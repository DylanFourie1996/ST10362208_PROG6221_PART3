using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ST10362208_PROG6221_PART3
{
    public delegate void CaloriesExceedsDelegate(); //(see Delegates in C# - a practical demonstration, including action and Func, 2018).

    public partial class MainWindow : Window
    {
        private CaptureRecipe captureRecipe = new CaptureRecipe();
        private RecipeFilter recipeFilter;
        private Dictionary<int, (string Name, List<string> Instructions, List<string> Ingredients, List<float> Quantities, List<int> Calories, List<string> FoodGroups)> originalRecipes = new Dictionary<int, (string, List<string>, List<string>, List<float>, List<int>, List<string>)>();

        public event CaloriesExceedsDelegate CaloriesExceedsEvent;

        public MainWindow()
        {
            InitializeComponent();

            
            List<CaptureRecipe> recipes = FetchRecipesFromDataSource(); 
            CaloriesExceedsEvent += HandleCaloriesExceeds;
            recipeFilter = new RecipeFilter(recipes);
            RecipeListBox.SelectionChanged += RecipeListBox_SelectionChanged;
            RefreshRecipeList();

        }

        private void HandleCaloriesExceeds() // Delegate 
        {
            MessageBox.Show("Total calories exceed 300!", "Calories Exceed Limit", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private List<CaptureRecipe> FetchRecipesFromDataSource()
        {
           
           
            return new List<CaptureRecipe>();  
        }
        private void AddRecipe_Click(object sender, RoutedEventArgs e) // ADD recipe (see C# programming • C# intermediate level • C# course • C# tutorials • C# basics • learn C# • (pt. 1), 2022)
                                                                     
        {
            string recipeName = RecipeNameTxt.Text.Trim();
            List<string> instructions = StepsTxt.Text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            List<string> ingredients = IngredientTxt.Text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            List<float> quantities = ParseFloatList(IngredientQuantityTxt.Text);
            List<int> calories;

            if (quantities == null)
            {
                MessageBox.Show("Invalid input for Ingredient Quantities. Please enter numeric values only.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                calories = CaloriesTxt.Text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();
            }
            catch (FormatException)
            {
                MessageBox.Show("Invalid input for Calories. Please enter numeric values only.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            List<string> foodGroups = FoodGroupComboBox.Items.Cast<ComboBoxItem>().Where(item => item.IsSelected).Select(item => item.Content.ToString()).ToList();

            // Calculate total calories for the entire recipe
            int totalCalories = CalculateTotalCalories(quantities, calories);

            // Capture recipe information
            captureRecipe.CaptureRecipeInformation(recipeName, instructions, ingredients, quantities, calories, foodGroups);

            // Refresh UI
            RefreshRecipeList();

            // Notify if calories exceed the limit
            if (totalCalories > 300)
            {
                CaloriesExceedsEvent?.Invoke();//(see Delegates in C# - a practical demonstration, including action and Func, 2018).
            }

            // Display recipe added message with total calories
            MessageBox.Show($"Recipe '{recipeName}' added successfully!\nTotal Calories: {totalCalories}", "Recipe Added", MessageBoxButton.OK, MessageBoxImage.Information);

            // Display details in RecipeDetailsTxt
            DisplayRecipeDetails(captureRecipe.Names.IndexOf(recipeName)); //()
        }




        private int CalculateTotalCalories(List<float> quantities, List<int> calories)
        {
            // Calculate and return total calories for the recipe
            int totalCalories = 0;
            for (int i = 0; i < quantities.Count; i++)
            {
                totalCalories += (int)(quantities[i] * calories[i]); // Multiply quantity by calories for each ingredient
            }
            return totalCalories;  // (see C# programming • C# intermediate level • C# course • C# tutorials • C# basics • learn C# • (pt. 2), 2022)
        }


        private void AddDetails_Click(object sender, RoutedEventArgs e)
        {
            if (RecipeListBox.SelectedItems.Count == 1)
            {
                string selectedRecipeName = RecipeListBox.SelectedItem.ToString();
                int index = captureRecipe.Names.IndexOf(selectedRecipeName);
                if (index >= 0)
                {
                    string ingredient = IngredientTxt.Text.Trim();
                    if (!float.TryParse(IngredientQuantityTxt.Text, out float quantity))
                    {
                        MessageBox.Show("Invalid input for Ingredient Quantity. Please enter a numeric value.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    if (!int.TryParse(CaloriesTxt.Text, out int calories))
                    {
                        MessageBox.Show("Invalid input for Calories. Please enter a numeric value.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    string foodGroup = (FoodGroupComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
                    string instruction = StepsTxt.Text.Trim();

                    // Append new details to existing recipe
                    captureRecipe.Ingredients[index].Add(ingredient);
                    captureRecipe.Units[index].Add(quantity);
                    captureRecipe.IngredientCalories[index].Add(calories);
                    if (!captureRecipe.IngredientFoodGroups[index].Contains(foodGroup))
                    {
                        captureRecipe.IngredientFoodGroups[index].Add(foodGroup);  // (see C# programming • C# intermediate level • C# course • C# tutorials • C# basics • learn C# • (pt. 2), 2022)
                    }
                    captureRecipe.Instructions[index].Add(instruction);

                    // Update total calories for the recipe
                    int totalCalories = captureRecipe.IngredientCalories[index].Sum();

                    // Refresh UI
                    RefreshRecipeList();

                    // Notify if calories exceed the limit
                    if (totalCalories > 300)
                    {
                        CaloriesExceedsEvent?.Invoke();
                    }

                    // Display updated recipe details
                    DisplayRecipeDetails(index);

                    MessageBox.Show($"Ingredient '{ingredient}' added to recipe '{captureRecipe.Names[index]}'.", "Ingredient Added", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("Please select exactly one recipe to add details to.", "Select One Recipe", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private List<float> ParseFloatList(string input)
        {
            List<float> result = new List<float>();
            string[] lines = input.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string line in lines)
            {
                if (float.TryParse(line, out float quantity))
                {
                    result.Add(quantity);
                }
                else
                {
                    return null; // Return null if any line fails to parse as float
                }
            }

            return result;
        }

        private void UpdateRecipe_Click(object sender, RoutedEventArgs e)
        {
            if (RecipeListBox.SelectedItems.Count == 1)
            {
                string selectedRecipeName = RecipeListBox.SelectedItem.ToString();
                int index = captureRecipe.Names.IndexOf(selectedRecipeName);
                if (index >= 0)
                {
                    List<float> quantities;
                    List<int> calories;

                    try
                    {
                        quantities = IngredientQuantityTxt.Text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(float.Parse).ToList();
                    }
                    catch (FormatException)
                    {
                        MessageBox.Show("Invalid input for Ingredient Quantities. Please enter numeric values only.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    try
                    {
                        calories = CaloriesTxt.Text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();
                    }
                    catch (FormatException)
                    {
                        MessageBox.Show("Invalid input for Calories. Please enter numeric values only.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Update recipe details
                    captureRecipe.Names[index] = RecipeNameTxt.Text.Trim();
                    captureRecipe.Instructions[index] = StepsTxt.Text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    captureRecipe.Ingredients[index] = IngredientTxt.Text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    captureRecipe.Units[index] = quantities;
                    captureRecipe.IngredientCalories[index] = calories;
                    captureRecipe.IngredientFoodGroups[index] = FoodGroupComboBox.Items.Cast<ComboBoxItem>().Where(item => item.IsSelected).Select(item => item.Content.ToString()).ToList();

                    // Calculate total calories
                    int totalCalories = captureRecipe.IngredientCalories[index].Sum();

                    RefreshRecipeList(); // Update RecipeListBox after updating recipe

                    // Display updated total calories and recipe details
                    DisplayRecipeDetails(index);

                    // Display updated total calories
                    MessageBox.Show($"Recipe '{captureRecipe.Names[index]}' updated successfully!\nTotal Calories: {totalCalories}", "Recipe Updated", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Trigger delegate if calories exceed 300
                    if (totalCalories > 300)
                    {
                        CaloriesExceedsEvent?.Invoke();
                    }  // (see C# programming • C# intermediate level • C# course • C# tutorials • C# basics • learn C# • (pt. 2), 2022)
                }
            }
            else
            {
                MessageBox.Show("Please select exactly one recipe to update.", "Select One Recipe", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        private void DeleteIndividualBtn_Click(object sender, RoutedEventArgs e)
        {
            if (RecipeListBox.SelectedItems.Count > 0)
            {
                List<string> selectedRecipes = RecipeListBox.SelectedItems.Cast<string>().ToList();
                selectedRecipes.ForEach(recipeName => DeleteFunctions.DeleteRecipeByName(recipeName, captureRecipe));

                RefreshRecipeList(); // Update RecipeListBox after deleting recipe(s)
            }
            else
            {
                MessageBox.Show("Please select at least one recipe to delete.", "No Recipe Selected", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void DeleteAllBtn_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to delete all recipes?", "Confirm Deletion", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                DeleteFunctions.DeleteAllRecipes(captureRecipe);
                RefreshRecipeList(); // Update RecipeListBox after deleting all recipes
            }
        }

        private void RecipeListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedRecipeName = RecipeListBox.SelectedItem as string;
            if (selectedRecipeName != null)
            {
                int index = captureRecipe.Names.IndexOf(selectedRecipeName);
                if (index >= 0)
                {
                    // Populate text boxes with selected recipe details
                    RecipeNameTxt.Text = captureRecipe.Names[index];
                    StepsTxt.Text = string.Join("\n", captureRecipe.Instructions[index]);
                    IngredientTxt.Text = string.Join("\n", captureRecipe.Ingredients[index]);
                    IngredientQuantityTxt.Text = string.Join("\n", captureRecipe.Units[index].Select(u => u.ToString()));
                    CaloriesTxt.Text = captureRecipe.TotalCalories[index].ToString();

                    // Clear and set FoodGroupComboBox
                    FoodGroupComboBox.SelectedIndex = -1;
                    foreach (var group in captureRecipe.IngredientFoodGroups[index])
                    {
                        var item = FoodGroupComboBox.Items.Cast<ComboBoxItem>().FirstOrDefault(i => i.Content.ToString() == group);
                        if (item != null)
                        {
                            item.IsSelected = true;
                        }
                    }

                    UpdateRecipeButton.IsEnabled = true;

                    // Display details in RecipeDetailsTxt if needed
                    DisplayRecipeDetails(index);
                }
            }
        }


        private void RecipeListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && RecipeListBox.SelectedItem != null)
            {
                string selectedRecipeName = RecipeListBox.SelectedItem.ToString();
                int index = captureRecipe.Names.IndexOf(selectedRecipeName);
                if (index >= 0)
                {
                    // Clear existing content
                    RecipeDetailsTxt.Document.Blocks.Clear();

                    // Append new content
                    string recipeDetails = $"Recipe Name: {captureRecipe.Names[index]}\n" +
                                           $"Instructions:\n{string.Join("\n", captureRecipe.Instructions[index])}\n" +
                                           $"Ingredients:\n{string.Join("\n", captureRecipe.Ingredients[index])}\n" +
                                           $"Quantities:\n{string.Join("\n", captureRecipe.Units[index].Select(u => u.ToString()))}\n" +
                                           $"Calories:\n{string.Join("\n", captureRecipe.IngredientCalories[index])}\n" +
                                           $"Total Calories: {captureRecipe.TotalCalories[index]}\n" +
                                           $"Food Groups:\n{string.Join(", ", captureRecipe.IngredientFoodGroups[index])}\n";

                    RecipeDetailsTxt.AppendText(recipeDetails);

                    // Debug output
                    Console.WriteLine($"Displaying recipe details for '{captureRecipe.Names[index]}'.");

                    RecipeDetailsTxt.Focus();
                }
            }
        }


        private void DisplayAllRecipes_Click(object sender, RoutedEventArgs e)
        {
            RefreshRecipeList();
        }

        private void RefreshRecipeList()
        {
            captureRecipe.Names.Sort();
            RecipeListBox.ItemsSource = null; // Clear the items source first
            RecipeListBox.ItemsSource = captureRecipe.Names;
        }


        private void DisplayRecipeDetails(int index)
        {
            RecipeDetailsTxt.Document.Blocks.Clear();

            // Initialize a StringBuilder to build the formatted recipe details
            StringBuilder recipeDetailsBuilder = new StringBuilder();

            // Append Recipe Name
            recipeDetailsBuilder.AppendLine($"Recipe Name: {captureRecipe.Names[index]}");

            // Append Ingredients and Quantities
            recipeDetailsBuilder.AppendLine("Ingredients:");
            for (int i = 0; i < captureRecipe.Ingredients[index].Count; i++)
            {
                recipeDetailsBuilder.AppendLine($"- {captureRecipe.Ingredients[index][i]}, Quantity: {captureRecipe.Units[index][i]}");
            }

            // Append Total Calories
            int totalCalories = captureRecipe.TotalCalories[index];
            recipeDetailsBuilder.AppendLine($"Total Calories: {totalCalories}");

            // Append Food Groups
            recipeDetailsBuilder.AppendLine("Food Groups: " + string.Join(", ", captureRecipe.IngredientFoodGroups[index]));

            // Append Instructions
            recipeDetailsBuilder.AppendLine("Instructions:");
            for (int i = 0; i < captureRecipe.Instructions[index].Count; i++)
            {
                recipeDetailsBuilder.AppendLine($"Step {i + 1}: {captureRecipe.Instructions[index][i]}");
            }

            // Append the built recipe details to the RecipeDetailsTxt RichTextBox
            RecipeDetailsTxt.AppendText(recipeDetailsBuilder.ToString());
        }




        private void DeleteRecipeButton_Click(object sender, RoutedEventArgs e)
        {
            // Get selected recipe name(s) from ListBox (assuming ListBox is named RecipeListBox)
            var selectedRecipes = RecipeListBox.SelectedItems.Cast<string>().ToList();

            // Delete each selected recipe
            foreach (var recipeName in selectedRecipes)
            {
                DeleteRecipe(recipeName);
            }

            // Refresh display after deleting selected recipes
            RefreshRecipeList();
        }

        private void DeleteAllRecipesButton_Click(object sender, RoutedEventArgs e)
        {
            // Delete all recipes
            DeleteAllRecipes();

            // Refresh display after deleting all recipes
            RefreshRecipeList();
        }

        private void DeleteRecipe(string recipeName)
        {
            // Call static method to delete recipe by name
            DeleteFunctions.DeleteRecipeByName(recipeName, captureRecipe);
        }

        private void DeleteAllRecipes()
        {
            // Call static method to delete all recipes
            DeleteFunctions.DeleteAllRecipes(captureRecipe);
        }
        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string searchQuery = SearchTextBox.Text.ToLower();

            // Filter recipes based on the search query
            var filteredRecipes = captureRecipe.Names.Where(name => name.ToLower().Contains(searchQuery)).ToList();

            // Update the RecipeListBox with the filtered recipes
            RecipeListBox.ItemsSource = filteredRecipes;
        }
        private void UpdateRecipeButton_Click(object sender, RoutedEventArgs e)
        {
            if (RecipeListBox.SelectedItems.Count == 1)
            {
                string selectedRecipeName = RecipeListBox.SelectedItem.ToString();
                int index = captureRecipe.Names.IndexOf(selectedRecipeName);
                if (index >= 0)
                {
                    // Store original recipe details before updating
                    StoreOriginalRecipeDetails(index);

                    // Update recipe details
                    captureRecipe.Names[index] = RecipeNameTxt.Text.Trim();
                    captureRecipe.Instructions[index] = StepsTxt.Text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    captureRecipe.Ingredients[index] = IngredientTxt.Text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    captureRecipe.Units[index] = ParseFloatList(IngredientQuantityTxt.Text);
                    captureRecipe.IngredientCalories[index] = CaloriesTxt.Text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();
                    captureRecipe.IngredientFoodGroups[index] = FoodGroupComboBox.Items.Cast<ComboBoxItem>().Where(item => item.IsSelected).Select(item => item.Content.ToString()).ToList();

                    // Calculate total calories
                    int totalCalories = CalculateTotalCalories(captureRecipe.Units[index], captureRecipe.IngredientCalories[index]);

                    // Refresh UI
                    RefreshRecipeList();
                    DisplayRecipeDetails(index);

                    // Notify if calories exceed the limit
                    if (totalCalories > 300)
                    {
                        CaloriesExceedsEvent?.Invoke();
                    }

                    MessageBox.Show($"Recipe '{captureRecipe.Names[index]}' updated successfully!\nTotal Calories: {totalCalories}", "Recipe Updated", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("Please select exactly one recipe to update.", "Select One Recipe", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void StoreOriginalRecipeDetails(int index)
        {
            // Store original recipe details in the dictionary
            originalRecipes[index] = (
                captureRecipe.Names[index],
                new List<string>(captureRecipe.Instructions[index]),
                new List<string>(captureRecipe.Ingredients[index]),
                new List<float>(captureRecipe.Units[index]),
                new List<int>(captureRecipe.IngredientCalories[index]),
                new List<string>(captureRecipe.IngredientFoodGroups[index])
            );
        }

        private string GetUpdatedFields(string originalName, List<string> originalInstructions, List<string> originalIngredients, List<float> originalQuantities, List<int> originalCalories, List<string> originalFoodGroups, CaptureRecipe updatedRecipe, int index)
        {
            string updatedFields = string.Empty;

            // Check each field for updates using switch staements
            switch (index)
            {
                case 0:
                    if (originalName != updatedRecipe.Names[index])
                    {
                        updatedFields += $"- Name: {originalName} -> {updatedRecipe.Names[index]}\n";
                    }
                    break;
                case 1:
                    if (!originalInstructions.SequenceEqual(updatedRecipe.Instructions[index]))
                    {
                        updatedFields += $"- Instructions: {string.Join(", ", originalInstructions)} -> {string.Join(", ", updatedRecipe.Instructions[index])}\n";
                    }
                    break;
                case 2:
                    if (!originalIngredients.SequenceEqual(updatedRecipe.Ingredients[index]))
                    {
                        updatedFields += $"- Ingredients: {string.Join(", ", originalIngredients)} -> {string.Join(", ", updatedRecipe.Ingredients[index])}\n";
                    }
                    break;
                case 3:
                    if (!originalQuantities.SequenceEqual(updatedRecipe.Units[index]))
                    {
                        updatedFields += $"- Quantities: {string.Join(", ", originalQuantities)} -> {string.Join(", ", updatedRecipe.Units[index])}\n";
                    }
                    break;
                case 4:
                    if (!originalCalories.SequenceEqual(updatedRecipe.IngredientCalories[index]))
                    {
                        updatedFields += $"- Calories: {string.Join(", ", originalCalories)} -> {string.Join(", ", updatedRecipe.IngredientCalories[index])}\n";
                    }
                    break;
                case 5:
                    if (!originalFoodGroups.SequenceEqual(updatedRecipe.IngredientFoodGroups[index]))
                    {
                        updatedFields += $"- Food Groups: {string.Join(", ", originalFoodGroups)} -> {string.Join(", ", updatedRecipe.IngredientFoodGroups[index])}\n";
                    }
                    break;
                default:
                    break;
            }

            return updatedFields;
        }
        private void ResetQuantities_Click(object sender, RoutedEventArgs e)
        {
            if (RecipeListBox.SelectedItems.Count == 1)
            {
                // Get the selected recipe name
                string selectedRecipeName = RecipeListBox.SelectedItem.ToString();
                int index = captureRecipe.Names.IndexOf(selectedRecipeName);

                // Check if the original details for this recipe exist
                if (index >= 0 && originalRecipes.ContainsKey(index))
                {
                    // Retrieve original recipe details
                    (
                        string originalName,
                        List<string> originalInstructions,
                        List<string> originalIngredients,
                        List<float> originalQuantities,
                        List<int> originalCalories,
                        List<string> originalFoodGroups
                    ) = originalRecipes[index];

                    // Update UI with original details
                    RecipeNameTxt.Text = originalName;
                    StepsTxt.Text = string.Join("\n", originalInstructions);
                    IngredientTxt.Text = string.Join("\n", originalIngredients);
                    IngredientQuantityTxt.Text = string.Join("\n", originalQuantities.Select(q => q.ToString()));
                    CaloriesTxt.Text = string.Join("\n", originalCalories);

                    // Clear and set FoodGroupComboBox
                    FoodGroupComboBox.SelectedIndex = -1;
                    foreach (var group in originalFoodGroups)
                    {
                        var item = FoodGroupComboBox.Items.Cast<ComboBoxItem>().FirstOrDefault(i => i.Content.ToString() == group);
                        if (item != null)
                        {
                            item.IsSelected = true;
                        }
                    }

                    // Disable update button until changes are made
                    UpdateRecipeButton.IsEnabled = false;
                }
            }
            else
            {
                MessageBox.Show("Please select exactly one recipe to reset.", "Select One Recipe", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}