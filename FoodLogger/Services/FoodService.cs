using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FoodLogger.Services
{
    public static class FoodService
    {
        public static List<string> GetFoods(string searchPhrase)
        {
            var foods = new List<string>();
            //a mock class that simulates a database search where several foods are returned based on a search phrase
            switch (searchPhrase)
            {
                case "bananas":
                    foods.Add("Raw Banana");
                    foods.Add("Banana Loaf");
                    foods.Add("Banana Yoghurt");
                    break;
                case "pastry":
                    foods.Add("Cinamon Danish Pastry");
                    foods.Add("Apple Pastry");
                    foods.Add("Croissant pastry");
                    break;
                case "coffee":
                    foods.Add("Cappuccino");
                    foods.Add("Americano");
                    foods.Add("Latte");
                    break;
            }
            return foods;
        }

        public static Boolean IsMealHealthy(IList<string> foods)
        {
            Random randomSeed = new Random();
            return (randomSeed.NextDouble() > 0.5);
        }
    }
}