using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using System.Text;

using Azure;
using Azure.AI.Vision.Common.Input;
using Azure.AI.Vision.Common.Options;
using Azure.AI.Vision.ImageAnalysis;

namespace GenerativeAIDemo.Pages
{
    public class ClassicVisionPageModel : PageModel
    {
        [BindProperty]
        public string Prompt { get; set; }
        public string Result { get; set; }

        public async Task<IActionResult> OnPostAnalyzeImage()
        {
            try
            {
                var serviceOptions = new VisionServiceOptions(
                    "https://cog-ms-learn-vision-labp.cognitiveservices.azure.com/",
                    new AzureKeyCredential(""));

                using var imageSource = VisionSource.FromUrl(Prompt);

                var analysisOptions = new ImageAnalysisOptions()
                {
                    Features = ImageAnalysisFeature.Caption
                        | ImageAnalysisFeature.DenseCaptions
                        | ImageAnalysisFeature.Text
                        | ImageAnalysisFeature.Tags
                        | ImageAnalysisFeature.Objects
                        | ImageAnalysisFeature.People,

                    Language = "en",
                    GenderNeutralCaption = true
                };

                using var analyzer = new ImageAnalyzer(serviceOptions, imageSource, analysisOptions);

                var result = await analyzer.AnalyzeAsync();
                var text = new StringBuilder();

                if (result.Reason == ImageAnalysisResultReason.Analyzed)
                {
                    if (result.Caption != null)
                    {
                        text.AppendLine($"----------- CAPTION --------------<br/>");
                        text.AppendLine($"\"{result.Caption.Content}\", Confidence {result.Caption.Confidence:0.0000}<br/>");
                        text.AppendLine($"----------------------------------------<br/>");
                    }

                    if (result.DenseCaptions != null)
                    {
                        text.AppendLine($"----------- DENSE CAPTIONS --------------\n");

                        foreach (var caption in result.DenseCaptions)
                        {
                            string pointsToString = "{" + caption.BoundingBox.ToString() + "}";
                            text.AppendLine($"'{caption.Content}', Confidence {caption.Confidence:0.0000}, Bounding box {pointsToString}\n");
                        }

                        text.AppendLine($"----------------------------------------\n");
                    }

                    if (result.Text != null)
                    {
                        text.AppendLine($"----------- TEXT --------------\n");

                        foreach (var line in result.Text.Lines)
                        {
                            string pointsToString = "{" + string.Join(',', line.BoundingPolygon.Select(pointsToString => pointsToString.ToString())) + "}";
                            text.AppendLine($"'{line.Content}', Bounding polygon {pointsToString}\n");
                        }

                        text.AppendLine($"----------------------------------------\n");
                    }

                    if (result.People != null)
                    {
                        text.AppendLine($"----------- PEOPLE --------------\n");

                        foreach (var people in result.People)
                        {
                            string pointsToString = "{" + people.BoundingBox.ToString() + "}";
                            text.AppendLine($"'Confidence {people.Confidence:0.0000}, Bounding box {pointsToString}\n");
                        }

                        text.AppendLine($"----------------------------------------\n");
                    }

                    if (result.People != null)
                    {
                        text.AppendLine($"----------- OBJECTS --------------\n");

                        foreach (var obj in result.Objects)
                        {
                            string pointsToString = "{" + obj.BoundingBox.ToString() + "}";
                            text.AppendLine($"'{obj.Name}', 'Confidence {obj.Confidence:0.0000}, Bounding box {pointsToString}\n");
                        }

                        text.AppendLine($"----------------------------------------\n");
                    }

                    if (result.Tags != null)
                    {
                        text.AppendLine($"----------- TAGS --------------\n");

                        foreach (var tag in result.Tags)
                        {
                            text.AppendLine($"'{tag.Name}', 'Confidence {tag.Confidence:0.0000}");
                        }

                        text.AppendLine($"----------------------------------------\n");
                    }
                }
                else
                {
                    var errorDetails = ImageAnalysisErrorDetails.FromResult(result);
                    text.AppendLine(" Analysis failed.\n");
                    text.AppendLine($" Error reason: {errorDetails.Reason}\n");
                    text.AppendLine($" Error code: {errorDetails.ErrorCode}\n");
                    text.AppendLine($" Error message: {errorDetails.Message}\n");
                }

                Result = text.ToString();
            }
            catch (Exception ex)
            {
                throw;
            }

            return Page();
        }

        public void OnGet()
        {
        }
    }
}
