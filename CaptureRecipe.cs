using System;
using System.Collections.Generic;
using System.Linq;

namespace ST10362208_PROG6221_PART3
{
    public class CaptureRecipe
    {
        // Private fields for storing recipe data
        private List<string> names = new List<string>();
        private List<List<string>> instructions = new List<List<string>>();
        private List<List<string>> ingredients = new List<List<string>>();
        private List<List<float>> units = new List<List<float>>();
        private List<List<int>> ingredientCalories = new List<List<int>>();
        private List<List<string>> ingredientFoodGroups = new List<List<string>>();
        private List<int> totalCalories = new List<int>();
        public double VolumeMultiplier { get; set; } = 1.0; // Default to 1.0 (original size)

        // Public property to get the first recipe name
        public string Name => Names?.FirstOrDefault();

        // Method to capture recipe information
        public void CaptureRecipeInformation(
            string recipeName,
            List<string> stepInstructions,
            List<string> ingredientNames,
            List<float> ingredientUnits,
            List<int> ingredientCalories,
            List<string> ingredientFoodGroupsList)
        {
            // Add each parameter to respective lists
            names.Add(recipeName);
            instructions.Add(stepInstructions);
            ingredients.Add(ingredientNames);
            units.Add(ingredientUnits);
            this.ingredientCalories.Add(ingredientCalories);
            ingredientFoodGroups.Add(ingredientFoodGroupsList);

            // Calculate total calories for the recipe
            int totalCal = CalculateTotalCalories(ingredientUnits, ingredientCalories);
            totalCalories.Add(totalCal);

            // Check for calorie warning
            CheckCalorieWarning(totalCal);
        }

        // Method to calculate total calories based on units and calorie values
        private int CalculateTotalCalories(List<float> quantities, List<int> calories)
        {
            if (quantities.Count != calories.Count)
                throw new ArgumentException("Quantities and calories lists must have the same number of elements.");

            int totalCalories = 0;
            for (int i = 0; i < quantities.Count; i++)
            {
                totalCalories += (int)(quantities[i] * calories[i]);
            }
            return totalCalories;
        }

        // Method to check if total calories exceed 300 and display a warning
        private void CheckCalorieWarning(int totalCalories)
        {
            if (totalCalories > 300)
            {
                Console.WriteLine($"Warning: Total calories ({totalCalories}) exceed 300!");
            }
        }

        // Properties to access recipe details
        public List<string> Names { get { return names; } }
        public List<List<string>> Instructions { get { return instructions; } }
        public List<List<string>> Ingredients { get { return ingredients; } }
        public List<List<float>> Units { get { return units; } }
        public List<List<int>> IngredientCalories { get { return ingredientCalories; } }
        public List<List<string>> IngredientFoodGroups { get { return ingredientFoodGroups; } }
        public List<int> TotalCalories { get { return totalCalories; } }
    }
}
