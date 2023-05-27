using GenerativeAIDemo.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GenerativeAIDemo.Pages
{
    public class AIPageModel : PageModel
    {
        [BindProperty]
        public string Prompt { get; set; }
        public string Result { get; set; }
        public bool IsText { get; set; }

        private OpenAIService service = new OpenAIService();

        public async Task<IActionResult> OnPostAskQuestion()
        {
            try
            {
                Result = await service.AskQuestion(Prompt);
                IsText = true;
            }
            catch (Exception ex)
            {
                throw;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostCreateImage()
        {
            try
            {
                Result = await service.CreateImage(Prompt);
                IsText = false;  
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
