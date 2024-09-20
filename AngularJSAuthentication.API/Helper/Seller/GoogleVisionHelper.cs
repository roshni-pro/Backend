using Google.Apis.Auth.OAuth2;
using Google.Apis.Http;
using Google.Apis.Services;
using Google.Apis.Vision.v1;
using Google.Apis.Vision.v1.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace AngularJSAuthentication.API.Helper.Seller
{

    public static class GoogleVisionHelper
    {
        /// <summary>
        /// Creates the credentials.
        /// </summary>
        /// <param name="path">The path to credential file.</param>
        /// <returns>The credentials.</returns>
        public static GoogleCredential CreateCredentials(string path)
        {
            GoogleCredential credential;
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                var c = GoogleCredential.FromStream(stream);
                credential = c.CreateScoped(VisionService.Scope.CloudPlatform);
            }

            return credential;
        }
        /// <summary>
        /// Creates the service.
        /// </summary>
        /// <param name="applicationName">Name of the application.</param>
        /// <param name="credentials">The credentials.</param>
        /// <returns>The service.</returns>
        public static VisionService CreateService(string applicationName, IConfigurableHttpClientInitializer credentials)
        {
            var service = new VisionService(
                new BaseClientService.Initializer()
                {
                    ApplicationName = applicationName,
                    HttpClientInitializer = credentials
                }
            );

            return service;
        }
        /// <summary>
        /// Creates the annotation image request.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="featureTypes">The feature types.</param>
        /// <returns>The request.</returns>
        private static AnnotateImageRequest CreateAnnotationImageRequest(string path, string[] featureTypes)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("Not found.", path);
            }

            var request = new AnnotateImageRequest();
            request.Image = new Image();

            var bytes = File.ReadAllBytes(path);
            request.Image.Content = Convert.ToBase64String(bytes);

            request.Features = new List<Feature>();

            foreach (var featureType in featureTypes)
            {
                request.Features.Add(new Feature() { Type = featureType });
            }

            return request;
        }

        /// <summary>
        /// Annotates the file asynchronously.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="file">The file.</param>
        /// <param name="features">The features.</param>
        /// <returns>The annotation response.</returns>
        public static async Task<AnnotateImageResponse> AnnotateAsync(this VisionService service, string fileurl, params string[] features)
        {
            var request = new BatchAnnotateImagesRequest();
            request.Requests = new List<AnnotateImageRequest>();
            request.Requests.Add(CreateAnnotationImageRequest(fileurl, features));

            var result = await service.Images.Annotate(request).ExecuteAsync();

            if (result?.Responses?.Count > 0)
            {
                return result.Responses[0];
            }

            return null;
        }

    }
}