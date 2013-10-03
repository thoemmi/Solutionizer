using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RestSharp;

namespace Solutionizer.Infrastructure {
    public interface IReleaseInfoReader {
        Task<IReadOnlyCollection<ReleaseInfo>> GetReleaseInfosAsync();
    }

    public class ReleaseInfo {
        public string Name { get; set; }
        public string ReleaseNotes { get; set; }
        public DateTimeOffset PublishedAt { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public string Filename { get; set; }
        public string DownloadUrl { get; set; }
    }

    public abstract class ReleaseInfoReaderBase : IReleaseInfoReader {
        public async Task<IReadOnlyCollection<ReleaseInfo>> GetReleaseInfosAsync() {
            var releases = await GetReleasesAsync();
            return releases.Select(r => r.ToReleaseInfo()).ToList();
        }

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
                    Name = Name,
                    ReleaseNotes = Body,
                    PublishedAt = PublishedAt,
                    CreatedAt = CreatedAt,
                };
                var asset = Assets.FirstOrDefault(a => a.Name.EndsWith(".msi"));
                if (asset != null) {
                    r.Filename = asset.Name;
                    r.DownloadUrl = asset.Url;
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

    public class FakeReleaseInfoReader : ReleaseInfoReaderBase {
        protected override Task<IEnumerable<Release>> GetReleasesAsync() {
            return Task.Run(() => {
                const string jsonString = @"[{""url"":""https://api.github.com/repos/thoemmi/Solutionizer/releases/5930"",""assets_url"":""https://api.github.com/repos/thoemmi/Solutionizer/releases/5930/assets"",""upload_url"":""https://uploads.github.com/repos/thoemmi/Solutionizer/releases/5930/assets{?name}"",""html_url"":""https://github.com/thoemmi/Solutionizer/releases/v0.1"",""id"":5930,""tag_name"":""v0.1"",""target_commitish"":""master"",""name"":"""",""body"":""Test"",""draft"":false,""prerelease"":false,""created_at"":""2012-10-01T07:38:14Z"",""published_at"":""2013-07-08T11:24:25Z"",""assets"":[]},{""url"":""https://api.github.com/repos/thoemmi/Solutionizer/releases/1904"",""assets_url"":""https://api.github.com/repos/thoemmi/Solutionizer/releases/1904/assets"",""upload_url"":""https://uploads.github.com/repos/thoemmi/Solutionizer/releases/1904/assets{?name}"",""html_url"":""https://github.com/thoemmi/Solutionizer/releases/v0.1.1"",""id"":1904,""tag_name"":""v0.1.1"",""target_commitish"":""master"",""name"":""Vanilla"",""body"":""This is a release for testing. **Don't use it!**"",""draft"":false,""prerelease"":true,""created_at"":""2012-11-04T17:06:04Z"",""published_at"":""2013-07-03T07:26:56Z"",""assets"":[{""url"":""https://api.github.com/repos/thoemmi/Solutionizer/releases/assets/673"",""id"":673,""name"":""solutionizer-0.1.1.70.msi"",""label"":""solutionizer-0.1.1.70.msi"",""content_type"":""application/octet-stream"",""state"":""uploaded"",""size"":1822720,""download_count"":0,""created_at"":""2013-07-03T07:25:30Z"",""updated_at"":""2013-07-03T07:26:56Z""}]}]";

                var deserializer = new RestSharp.Deserializers.JsonDeserializer();
                IEnumerable<Release> releases = deserializer.Deserialize<List<Release>>(new RestResponse { Content = jsonString });
                return releases;
            });
        }
    }

    public class GithubReleaseInfoReader : ReleaseInfoReaderBase {
        protected override async Task<IEnumerable<Release>> GetReleasesAsync() {
            var client = new RestClient("https://api.github.com");
            var request = new RestRequest("repos/thoemmi/Solutionizer/releases");
            request.AddHeader("Accept", "application/vnd.github.manifold-preview");
            var response = await client.ExecuteGetTaskAsync<List<Release>>(request);
            return response.Data;
        }
    }
}