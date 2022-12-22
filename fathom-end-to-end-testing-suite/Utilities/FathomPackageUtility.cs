using fathom_end_to_end_testing_suite.Infrastructure;
using fathom_end_to_end_testing_suite.Steps;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TechTalk.SpecFlow;

namespace fathom_end_to_end_testing_suite.Utilities
{

    class FathomPackageUtility : FathomApiClientBase
    {
        public FathomPackageUtility(string environment, ScenarioContext scenarioContext)
            : this(environment, scenarioContext[CONTEXT_KEYS.ACCESS_TOKEN].ToString())
        {
        }

        public FathomPackageUtility(string environment, string accessToken) : base(environment, accessToken)
        {
            EndPoint = $"{Environment}/package";
        }

        private string GetEndPointWithDataset(string datasetNameOrId) => GetEndPointUrilWithDataset(datasetNameOrId, "package");
        public string EndPoint { get; }

        public Result ApplyPackageFromFile(string datasetNameOrId, string packageFilePath)
        {
            var packageId = Path.GetFileNameWithoutExtension(packageFilePath);

            if (ParseVersion(packageId).IsFailure) packageId = "AdHocPackage.1.0";

            var packageDef = string.Empty;

            try
            {
                packageDef = File.ReadAllText(packageFilePath);
            }
            catch (Exception e)
            {
                return Result.Fail($"File {packageFilePath} could not be opened. {e.GetAllMessages()}");
            }

            var body = new Dictionary<string, string>();
            body.Add("packageId", packageId);
            body.Add("source", "Upload");
            body.Add("target", "DataSet");
            body.Add("targetDatasetId", datasetNameOrId);
            body.Add("objectDefinitionsInJson", packageDef);


            return Post(EndPoint, body, System.Net.HttpStatusCode.Created);
        }

        public string GetPackageName(string packageVersion) => packageVersion.Split('.').First();


        public Result<IEnumerable<string>> GetVersions()
        {
            var response = HttpUtility.GetAsync(EndPoint, AccessToken, GetDefaultHeaders(), GetDefaultQueryParams()).Result;

            if (response.HeaderResponseStatus != System.Net.HttpStatusCode.OK)
                return Result.Fail<IEnumerable<string>>($"Status: {response.HeaderResponseStatus}");

            return Result.Ok(Deserialize<IEnumerable<string>>(response.HttpResponseMessage));
        }


        public Result<bool> IsGreaterOrEqual(string version1, string version2)
        {
            return ParseVersions(version1, version2).AndThen((versions) =>
            {
                if (versions.major1 >= versions.major2 ||
                    (versions.major1 == versions.major2 && versions.minor1 >= versions.minor2)
                )
                    return Result.Ok(true);

                return Result.Ok(false);
            });
        }

        public Result<bool> IsLessOrEqual(string version1, string version2)
        {
            return ParseVersions(version1, version2).AndThen((versions) =>
            {
                if (versions.major1 <= versions.major2 ||
                    (versions.major1 == versions.major2 && versions.minor1 <= versions.minor2)
                )
                    return Result.Ok(true);

                return Result.Ok(false);
            });
        }

        private Result<(string name, int major1, int major2, int minor1, int minor2)> ParseVersions(string version1, string version2)
        {
            var errors = new List<string>();

            var v1ParseResult = ParseVersion(version1);
            var v2ParseResult = ParseVersion(version2);

            if (v1ParseResult.IsFailure)
                errors.Add(v1ParseResult.Error);

            if (v2ParseResult.IsFailure)
                errors.Add(v2ParseResult.Error);

            if (errors.Any())
                return Result.Fail<(string, int, int, int, int)>(string.Join(". ", errors).Trim());

            var v1 = v1ParseResult.Value;
            var v2 = v2ParseResult.Value;

            if (!v1.name.Equals(v2.name, StringComparison.OrdinalIgnoreCase))
                return Result.Fail<(string, int, int, int, int)>("Versions are incompatible. Check package name");

            return Result.Ok((v1.name, v1.major, v2.major, v1.minor, v2.minor));

        }

        public Result<string> GetLatestVersion(string packageName)
        {
            return GetVersions().AndThen((versions) =>
            {
                try
                {
                    var packageVersions = versions
                      .Where(v => v.StartsWith(packageName, System.StringComparison.OrdinalIgnoreCase))
                      .Select(v => v.Split('.'))
                      .Select(v => v.Skip(1).ToList())
                      .Select(v => v.Select(item => int.Parse(item)).ToList())
                      .OrderByDescending(v => v[0]).ThenByDescending(v => v[1]);

                    if (packageVersions.Any())
                    {
                        var latest = packageVersions.First();
                        return Result.Ok($"{packageName}.{latest[0]}.{latest[1]}");
                    }


                    return Result.Fail<string>($"No version found for {packageName}");
                }
                catch (System.Exception e)
                {
                    return Result.Fail<string>($"Latest version could not be identified. {e.GetAllMessages()}");
                }
            });
        }

        private Result<(string name, int major, int minor)> ParseVersion(string version)
        {
            var parts = version.Split('.');

            if (parts.Length != 3)
                return Result.Fail<(string, int, int)>($"Version {version} is not properly formatted");

            return Result.Ok((parts[0], int.Parse(parts[1]), int.Parse(parts[2])));
        }
    }
}
