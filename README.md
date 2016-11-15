# Bot-Disambiguation-Sample
A sample of how to disambiguate LUIS entities in the Bot Framework.

Given a phrase like "I had 2 bananas, a pastry and a coffee", the bot will use LUIS to get "bananas", "pastry" and "coffee" as food entities, the bot will then disambiguate each entity by giving the user choices of specific food items that were returned by searching on that entity.

Uses Bot Framework 3.0 with c#
