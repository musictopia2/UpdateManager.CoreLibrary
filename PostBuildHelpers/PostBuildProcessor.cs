using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace UpdateManager.CoreLibrary.PostBuildHelpers;
public static class PostBuildProcessor
{
    public static async Task ProcessPostBuildAsync(string[] args, Func<PostBuildArguments, Task> postBuildAction)
    {
        try
        {
            // Validate the arguments
            ArgumentValidator.ValidateArguments(args, 4);
            // Extract the arguments into a model
            var postBuildArgs = new PostBuildArguments(args[0], args[1], args[2], args[3]);
            // Return the processed arguments back to the caller
            //return postBuildArgs;
            await postBuildAction.Invoke(postBuildArgs);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Environment.Exit(1); // Exit on error
            //return null; // Never actually gets here, but required to compile
        }
    }
}
