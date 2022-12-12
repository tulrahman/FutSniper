﻿using FutSniper.Entities;
using FutSniper.Managers;
using System;
using System.Threading.Tasks;

namespace FutSniper
{
    public class Program
    {
        public async static Task Main()
        {
            WebAppManager webAppManager = null;

            try
            {
                Console.WriteLine("FUT Sniper started.");

                webAppManager = new WebAppManager(maximizeWindow: false);

                await MainImplementation(webAppManager);
            }
            catch (Exception e)
            {
                // TODO: Protect all null references.
                Console.Error.WriteLine($"An unexpected error occurred: {e.Message}");
            }
            finally
            {
                webAppManager?.Dispose();
            }
        }

        private static async Task MainImplementation(WebAppManager webAppManager)
        {
            Console.WriteLine("Waiting for sing in...");

            await webAppManager.WaitForSingIn();

            Console.WriteLine("Sing in completed successfully. Main page reached.");

            await webAppManager.EnterTransfersMarket();

            while (true)
            {
                try
                {
                    await FindPlayerAndTryToBuyIt(webAppManager);

                    await webAppManager.ExitFromSearchResults();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);

                    try
                    {
                        await webAppManager.EnterTransfersMarket();
                    }
                    catch
                    {
                        // It is not always needed to enter to transfers market. In addition, this
                        // action sometimes can fail.
                    }
                }
            }
        }

        private static async Task FindPlayerAndTryToBuyIt(WebAppManager webAppManager)
        {
            WantedPlayer wantedPlayer = WantedPlayersObtainer.GetWantedPlayer();

            await webAppManager.FindWantedPlayer(wantedPlayer.Name);

            await webAppManager.SetMaximumPrice(wantedPlayer.MaxPurchasePrice);

            await webAppManager.MakeSearch();

            string foundedPlayerPurchasePrice = await webAppManager.CheckPlayerPurchasePriceIfFounded();

            if (foundedPlayerPurchasePrice == null)
            {
                Console.WriteLine($"[{DateTime.Now}]: No players found.");

                return;
            }

            string baseWantedPlayerMessage = $"[{DateTime.Now}]: {wantedPlayer.Name} is wanted under {wantedPlayer.MaxPurchasePrice} coins";

            Console.WriteLine($"{baseWantedPlayerMessage} and it was found for {foundedPlayerPurchasePrice} coins");

            if (Convert.ToInt32(foundedPlayerPurchasePrice) > wantedPlayer.MaxPurchasePrice)
            {
                Console.Error.WriteLine("Something was wrong. Player was founded with a higher price that wanted.");
            }

            bool playerBought = await webAppManager.TryToBuyPlayer();

            if (playerBought)
            {
                // TODO: Save to a logs file.
                Console.Beep();
                Console.WriteLine($"{baseWantedPlayerMessage} and it was bought for {foundedPlayerPurchasePrice} coins");
            }
        }
    }
}