using System;
using System.Collections.Generic;
using System.Linq;

namespace ST10362208_PROG6221_PART3
{
    public class RecipeFilter
    {
        private List<CaptureRecipe> recipes;

        public RecipeFilter(List<CaptureRecipe> recipes)
        {
            this.recipes = recipes;
        }

        public List<CaptureRecipe> FilterRecipes(string nameFilter) // fitler Recipe names
        {
            var filteredRecipes = recipes.ToList();

            if (!string.IsNullOrEmpty(nameFilter))
            {
                filteredRecipes = filteredRecipes.Where(r => r.Names.Any(name => name.Contains(nameFilter))).ToList();
            }

            return filteredRecipes;
        }

        public List<CaptureRecipe> GetAllRecipes()
        {
            return recipes;
        }
    }
}