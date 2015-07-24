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
using Octokit;
using Solutionizer.Services;

namespace Solutionizer.Infrastructure {
    public interface IReleaseProvider {
        Task<IReadOnlyCollection<ReleaseInfo>> GetReleaseInfosAsync();

        Task<string> DownloadReleasePackage(ReleaseInfo releaseInfo, Action<int> downloadProgressCallback, CancellationToken cancellationToken);
    }

    public class ReleaseInfo {
        public string Name { get; set; }
        public string ReleaseNotes { get; set; }
        public DateTimeOffset? PublishedAt { get; set; }
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
            return releases.ToList();
        }

        public abstract Task<string> DownloadReleasePackage(ReleaseInfo releaseInfo, Action<int> downloadProgressCallback, CancellationToken cancellationToken);

        protected abstract Task<List<ReleaseInfo>> GetReleasesAsync();
    }
    
//    public class FakeReleaseProvider : ReleaseProviderBase {
//        protected override Task<IEnumerable<Release>> GetReleasesAsync() {
//            return Task.Run(() => {
//                const string jsonString = @"
//[
//    {
//        ""url"":""https://api.github.com/repos/thoemmi/Solutionizer/releases/5930"",
//        ""assets_url"":""https://api.github.com/repos/thoemmi/Solutionizer/releases/5930/assets"",
//        ""upload_url"":""https://uploads.github.com/repos/thoemmi/Solutionizer/releases/5930/assets{?name}"",
//        ""html_url"":""https://github.com/thoemmi/Solutionizer/releases/v0.1"",
//        ""id"":5930,
//        ""tag_name"":""v0.1"",
//        ""target_commitish"":""master"",
//        ""name"":"""",
//        ""body"":""Test"",
//        ""draft"":false,
//        ""prerelease"":false,
//        ""created_at"":""2012-10-01T07:38:14Z"",
//        ""published_at"":""2013-07-08T11:24:25Z"",
//        ""assets"":[]
//    },{
//        ""url"":""https://api.github.com/repos/thoemmi/Solutionizer/releases/1904"",
//        ""assets_url"":""https://api.github.com/repos/thoemmi/Solutionizer/releases/1904/assets"",
//        ""upload_url"":""https://uploads.github.com/repos/thoemmi/Solutionizer/releases/1904/assets{?name}"",
//        ""html_url"":""https://github.com/thoemmi/Solutionizer/releases/v0.1.1"",
//        ""id"":1904,
//        ""tag_name"":""v0.2.1"",
//        ""target_commitish"":""master"",
//        ""name"":""Vanilla"",
//        ""body"":""This is a release for testing. **Don't use it!**
//
//* Item 1
//* Item 2 [Website](http://thomasfreudenberg.com)"",
//        ""draft"":false,
//        ""prerelease"":true,
//        ""created_at"":""2012-11-04T17:06:04Z"",
//        ""published_at"":""2013-07-03T07:26:56Z"",
//        ""assets"":[
//            {
//                ""url"":""https://api.github.com/repos/thoemmi/Solutionizer/releases/assets/673"",
//                ""id"":673,
//                ""name"":""solutionizer-0.1.1.70.msi"",
//                ""label"":""solutionizer-0.1.1.70.msi"",
//                ""content_type"":""application/octet-stream"",
//                ""state"":""uploaded"",
//                ""size"":1822720,
//                ""download_count"":0,
//                ""created_at"":""2013-07-03T07:25:30Z"",
//                ""updated_at"":""2013-07-03T07:26:56Z""
//            }
//        ]
//}]";

//                var deserializer = new JsonDeserializer();
//                IEnumerable<Release> releases = deserializer.Deserialize<List<Release>>(new RestResponse { Content = jsonString });
//                return releases;
//            });
//        }

//        public async override Task<string> DownloadReleasePackage(ReleaseInfo releaseInfo, Action<int> downloadProgressCallback, CancellationToken cancellationToken) {
//            for (var i = 0; i < 50; ++i) {
//                if (downloadProgressCallback != null) {
//                    downloadProgressCallback(i*2);
//                }
//                if (cancellationToken.IsCancellationRequested) {
//                    return null;
//                }
//                await Task.Delay(100, cancellationToken);
//            }
//            return String.Empty;
//        }
//    }

    public class GithubReleaseProvider : ReleaseProviderBase {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();
        private const string GitHubOwner = "thoemmi";
        private const string GitHubRepository = "Solutionizer";

        private readonly ISettings _settings;

        public GithubReleaseProvider(ISettings settings) {
            _settings = settings;
        }

        protected override async Task<List<ReleaseInfo>> GetReleasesAsync() {
            var github = new GitHubClient(new ProductHeaderValue("Solutionizer", AppEnvironment.CurrentVersion.ToString()));
            var result = new List<ReleaseInfo>();
            foreach (var release in await github.Release.GetAll(GitHubOwner, GitHubRepository)) {
                var r = new ReleaseInfo {
                    Name = String.IsNullOrWhiteSpace(release.Name) ? release.TagName : release.Name,
                    ReleaseNotes = release.Body,
                    PublishedAt = release.PublishedAt,
                    CreatedAt = release.CreatedAt,
                    HtmlUrl = release.HtmlUrl,
                    TagName = release.TagName,
                    IsPrerelease = release.Prerelease,
                };

                var assets = await github.Release.GetAllAssets(GitHubOwner, GitHubRepository, release.Id);
                var asset = assets.FirstOrDefault(a => a.Name.EndsWith(".msi"));
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
                result.Add(r);
            }
            return result;
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