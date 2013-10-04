using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NLog;
using RestSharp;
using RestSharp.Deserializers;
using Solutionizer.Services;

namespace Solutionizer.Infrastructure {
    public interface IReleaseProvider {
        Task<IReadOnlyCollection<ReleaseInfo>> GetReleaseInfosAsync();

        Task<string> DownloadReleasePackage(ReleaseInfo releaseInfo, Action<int> downloadProgressCallback, CancellationToken cancellationToken);
    }

    public class ReleaseInfo {
        public string Name { get; set; }
        public string ReleaseNotes { get; set; }
        public DateTimeOffset PublishedAt { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public string Filename { get; set; }
        public string DownloadUrl { get; set; }
        public string HtmlUrl { get; set; }
        public string TagName { get; set; }
        public bool IsPrerelease { get; set; }
        [JsonConverter(typeof(VersionConverter))]
        public Version Version { get; set; }
        [JsonIgnore]
        public bool IsNew { get; set; }
    }

    public abstract class ReleaseProviderBase : IReleaseProvider {
        public async Task<IReadOnlyCollection<ReleaseInfo>> GetReleaseInfosAsync() {
            var releases = await GetReleasesAsync();
            return releases.Select(r => r.ToReleaseInfo()).ToList();
        }

        public abstract Task<string> DownloadReleasePackage(ReleaseInfo releaseInfo, Action<int> downloadProgressCallback, CancellationToken cancellationToken);

        public class Release {
            public string Url { get; set; }
            public string AssetsUrl { get; set; }
            public string UploadUrl { get; set; }
            public string HtmlUrl { get; set; }
            public int Id { get; set; }
            public string TagName { get; set; }
            public string TargetCommitish { get; set; }
            public string Name { get; set; }
            public string Body { get; set; }
            public bool Draft { get; set; }
            public bool Prerelease { get; set; }
            public DateTimeOffset CreatedAt { get; set; }
            public DateTimeOffset PublishedAt { get; set; }
            public List<Asset> Assets { get; set; }

            public ReleaseInfo ToReleaseInfo() {
                var r = new ReleaseInfo {
                    Name = String.IsNullOrWhiteSpace(Name) ? TagName : Name,
                    ReleaseNotes = Body,
                    PublishedAt = PublishedAt,
                    CreatedAt = CreatedAt,
                    HtmlUrl = HtmlUrl,
                    TagName = TagName,
                    IsPrerelease = Prerelease,
                };
                var asset = Assets.FirstOrDefault(a => a.Name.EndsWith(".msi"));
                if (asset != null) {
                    r.Filename = asset.Name;
                    r.DownloadUrl = asset.Url;
                }

                var match = Regex.Match(r.TagName, @"^v(?<major>\d+)\.(?<minor>\d+)(\.(?<patch>\d+))?$");
                if (match.Success) {
                    int major, minor, patch;
                    Int32.TryParse(match.Groups["major"].Value, out major);
                    Int32.TryParse(match.Groups["minor"].Value, out minor);
                    if (Int32.TryParse(match.Groups["patch"].Value, out patch)) {
                        r.Version = new Version(major, minor, patch);
                    } else {
                        r.Version = new Version(major, minor);
                    }
                }

                return r;
            }
        }

        public class Asset {
            public string Url { get; set; }
            public int Id { get; set; }
            public string Name { get; set; }
            public string Label { get; set; }
            public string ContentType { get; set; }
            public string State { get; set; }
            public int Size { get; set; }
            public int DownloadCount { get; set; }
            public DateTimeOffset CreatedAt { get; set; }
            public DateTimeOffset UpdatedAt { get; set; }
        }

        protected abstract Task<IEnumerable<Release>> GetReleasesAsync();
    }

    public class FakeReleaseProvider : ReleaseProviderBase {
        protected override Task<IEnumerable<Release>> GetReleasesAsync() {
            return Task.Run(() => {
                const string jsonString = @"
[
    {
        ""url"":""https://api.github.com/repos/thoemmi/Solutionizer/releases/5930"",
        ""assets_url"":""https://api.github.com/repos/thoemmi/Solutionizer/releases/5930/assets"",
        ""upload_url"":""https://uploads.github.com/repos/thoemmi/Solutionizer/releases/5930/assets{?name}"",
        ""html_url"":""https://github.com/thoemmi/Solutionizer/releases/v0.1"",
        ""id"":5930,
        ""tag_name"":""v0.1"",
        ""target_commitish"":""master"",
        ""name"":"""",
        ""body"":""Test"",
        ""draft"":false,
        ""prerelease"":false,
        ""created_at"":""2012-10-01T07:38:14Z"",
        ""published_at"":""2013-07-08T11:24:25Z"",
        ""assets"":[]
    },{
        ""url"":""https://api.github.com/repos/thoemmi/Solutionizer/releases/1904"",
        ""assets_url"":""https://api.github.com/repos/thoemmi/Solutionizer/releases/1904/assets"",
        ""upload_url"":""https://uploads.github.com/repos/thoemmi/Solutionizer/releases/1904/assets{?name}"",
        ""html_url"":""https://github.com/thoemmi/Solutionizer/releases/v0.1.1"",
        ""id"":1904,
        ""tag_name"":""v0.2.1"",
        ""target_commitish"":""master"",
        ""name"":""Vanilla"",
        ""body"":""This is a release for testing. **Don't use it!**

* Item 1
* Item 2 [Website](http://thomasfreudenberg.com)"",
        ""draft"":false,
        ""prerelease"":true,
        ""created_at"":""2012-11-04T17:06:04Z"",
        ""published_at"":""2013-07-03T07:26:56Z"",
        ""assets"":[
            {
                ""url"":""https://api.github.com/repos/thoemmi/Solutionizer/releases/assets/673"",
                ""id"":673,
                ""name"":""solutionizer-0.1.1.70.msi"",
                ""label"":""solutionizer-0.1.1.70.msi"",
                ""content_type"":""application/octet-stream"",
                ""state"":""uploaded"",
                ""size"":1822720,
                ""download_count"":0,
                ""created_at"":""2013-07-03T07:25:30Z"",
                ""updated_at"":""2013-07-03T07:26:56Z""
            }
        ]
}]";

                var deserializer = new JsonDeserializer();
                IEnumerable<Release> releases = deserializer.Deserialize<List<Release>>(new RestResponse { Content = jsonString });
                return releases;
            });
        }

        public override Task<string> DownloadReleasePackage(ReleaseInfo releaseInfo, Action<int> downloadProgressCallback, CancellationToken cancellationToken) {
            return Task.Run(() => {
                for (var i = 0; i < 50; ++i) {
                    if (downloadProgressCallback != null) {
                        downloadProgressCallback(i*2);
                    }
                    if (cancellationToken.IsCancellationRequested) {
                        return null;
                    }
                    Thread.Sleep(100);
                }
                return String.Empty;
            });
        }
    }

    public class GithubReleaseProvider : ReleaseProviderBase {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        private readonly ISettings _settings;

        public GithubReleaseProvider(ISettings settings) {
            _settings = settings;
        }

        protected override async Task<IEnumerable<Release>> GetReleasesAsync() {
            var client = new RestClient("https://api.github.com");
            var request = new RestRequest("repos/thoemmi/Solutionizer/releases");
            request.AddHeader("Accept", "application/vnd.github.manifold-preview");
            if (!String.IsNullOrWhiteSpace(_settings.LastUpdateCheck)) {
                request.AddHeader("If-Modified-Since", _settings.LastUpdateCheck);
            }
            var response = await client.ExecuteGetTaskAsync<List<Release>>(request);
            var lastModifiedPartameter = response.Headers.FirstOrDefault(p => p.Name == "Last-Modified");
            if (lastModifiedPartameter != null) {
                _settings.LastUpdateCheck = (string) lastModifiedPartameter.Value;
            }
            return response.Data ?? Enumerable.Empty<Release>();
        }

        public override async Task<string> DownloadReleasePackage(ReleaseInfo releaseInfo, Action<int> downloadProgressCallback, CancellationToken cancellationToken) {
            var destination = Path.Combine(Path.GetTempPath(), releaseInfo.Filename);
            if (File.Exists(destination)) {
                File.Delete(destination);
                Thread.Sleep(100);
            }
            using (var webClient = new WebClient()) {
                webClient.Headers.Add("Accept", "application/octet-stream");
                if (downloadProgressCallback != null) {
                    webClient.DownloadProgressChanged += (sender, args) => downloadProgressCallback(args.ProgressPercentage);
                }
                cancellationToken.Register(webClient.CancelAsync);
                _log.Debug("Downloading release from {0} to {1}", releaseInfo.DownloadUrl, destination);
                await webClient.DownloadFileTaskAsync(releaseInfo.DownloadUrl, destination);
            }
            return destination;
        }
    }
}