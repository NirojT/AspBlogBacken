using Domain.Blog.dto;
using Domain.Blog.entity;

namespace Presentation.Blog.utility
{
    public class FileHelper
    {
        public const string BaseUrl = "https://localhost:7064/";
        //save image
        public static async Task<string?> SaveImage(IFormFile imageFile, IWebHostEnvironment hostingEnvironment)
        {
            try
            {

                Console.WriteLine("hosting environment is " + hostingEnvironment);
                if (imageFile != null && imageFile.Length > 0)
                {
                    
                    // Get the absolute path to the Images folder
                    string imagePath = Path.Combine(hostingEnvironment.ContentRootPath, "Stores/Images");

                     
                    // Check if the directory exists, if not, create it
                    if (!Directory.Exists(imagePath))
                    {
                        Directory.CreateDirectory(imagePath);
                    }

                    // Generate a unique filename
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                    
                    // Combine the absolute path with the unique filename
                    string fullPath = Path.Combine(imagePath, uniqueFileName);
                    Console.WriteLine("fullPath is " + fullPath);
                    // Save the file to disk
                    await using (Stream stream = new FileStream(fullPath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }

                    // Return the URL of the saved image
                    return FileHelper.BaseUrl + "Stores/Images/" + uniqueFileName;
                }

                return null;
            }
            catch (Exception ex)
            {
                // Handle the exception (log, throw, etc.)
                Console.WriteLine($"Error saving image: {ex.Message}");
                return null;
            }


           
        }


     
    }
}
