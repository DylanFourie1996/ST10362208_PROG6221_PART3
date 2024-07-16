using System;
using System.Collections.Generic;
using System.Linq;

namespace ST10362208_PROG6221_PART3
{
    internal class DeleteFunctions
    {
        // Method to delete a specific recipe by name
        public static void DeleteRecipeByName(string recipeName, CaptureRecipe captureRecipe)
        {
            // Find index of recipe name in names list
            int index = captureRecipe.Names.IndexOf(recipeName);

            // If recipe name exists, remove all related data at the same index
            if (index != -1)
            {
                captureRecipe.Names.RemoveAt(index);
                captureRecipe.Instructions.RemoveAt(index);
                captureRecipe.Ingredients.RemoveAt(index);
                captureRecipe.Units.RemoveAt(index);
                captureRecipe.IngredientCalories.RemoveAt(index);
                captureRecipe.IngredientFoodGroups.RemoveAt(index);
                captureRecipe.TotalCalories.RemoveAt(index);
            }
            else
            {
            }
        }

        // Method to delete all recipes
        public static void DeleteAllRecipes(CaptureRecipe captureRecipe)
        {
            captureRecipe.Names.Clear();
            captureRecipe.Instructions.Clear();
            captureRecipe.Ingredients.Clear();
            captureRecipe.Units.Clear();
            captureRecipe.IngredientCalories.Clear();
            captureRecipe.IngredientFoodGroups.Clear();
            captureRecipe.TotalCalories.Clear();
        }
    }
}