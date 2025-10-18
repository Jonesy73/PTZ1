#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

namespace NinjaTrader.NinjaScript.Strategies
{
	public class PTZDailyPlanStrategy : Strategy
	{
		private NinjaTrader.NinjaScript.Indicators.PropTraderz.PTZDailyPlanURLv2 ptzIndicator;

		private double lastCheckPrice;
		private DateTime lastCheckTime;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Trading strategy based on PTZ Daily Plan levels - buys at support/pivot bull levels, sells at resistance/pivot bear levels";
				Name										= "PTZ Daily Plan Strategy";
				Calculate									= Calculate.OnBarClose;
				EntriesPerDirection							= 1;
				EntryHandling								= EntryHandling.AllEntries;
				IsExitOnSessionCloseStrategy				= true;
				ExitOnSessionCloseSeconds					= 30;
				IsFillLimitOnTouch							= false;
				MaximumBarsLookBack							= MaximumBarsLookBack.TwoHundredFiftySix;
				OrderFillResolution							= OrderFillResolution.Standard;
				Slippage									= 0;
				StartBehavior								= StartBehavior.WaitUntilFlat;
				TimeInForce									= TimeInForce.Gtc;
				TraceOrders									= false;
				RealtimeErrorHandling						= RealtimeErrorHandling.StopCancelClose;
				StopTargetHandling							= StopTargetHandling.PerEntryExecution;
				BarsRequiredToTrade							= 20;
				IsInstantiatedOnEachOptimizationIteration	= true;

				// Strategy Parameters
				UseSupport									= true;
				UseResistance								= true;
				UsePivotBull								= true;
				UsePivotBear								= true;
				UseStrengthConfirmed						= false;
				UseWeaknessConfirmed						= false;

				PriceProximityTicks							= 2;
				TradeOnCrossover							= true;
				TradeOnTouch								= true;

				EnableStopLoss								= false;
				StopLossTicks								= 20;
				EnableProfitTarget							= false;
				ProfitTargetTicks							= 40;

				EnableTrailStop								= false;
				TrailStopTicks								= 15;

				// Keywords (must match indicator settings)
				KeywordSupport								= "Support";
				KeywordResistance							= "Resistance";
				KeywordPivotBull							= "Pivot Bull";
				KeywordPivotBear							= "Pivot Bear";
				KeywordStrengthConfirmed					= "Strength Confirmed";
				KeywordWeaknessConfirmed					= "Weakness Confirmed";
			}
			else if (State == State.Configure)
			{
			}
			else if (State == State.DataLoaded)
			{
				// Initialize the PTZ Daily Plan indicator
				ptzIndicator = PTZDailyPlanURLv2();
				AddChartIndicator(ptzIndicator);

				lastCheckPrice = 0;
				lastCheckTime = DateTime.MinValue;
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar < BarsRequiredToTrade)
				return;

			if (ptzIndicator == null)
				return;

			// Get current price
			double currentPrice = Close[0];
			double previousPrice = Close[1];

			// Check for price level interactions
			CheckForBuySignals(currentPrice, previousPrice);
			CheckForSellSignals(currentPrice, previousPrice);

			// Update last check values
			lastCheckPrice = currentPrice;
			lastCheckTime = Time[0];
		}

		private void CheckForBuySignals(double currentPrice, double previousPrice)
		{
			// Don't enter new long if already long
			if (Position.MarketPosition == MarketPosition.Long)
				return;

			// Check for buy conditions based on level descriptions
			if (ShouldBuyAtLevel(currentPrice, previousPrice))
			{
				EnterLong("Buy at Support/Pivot Bull");

				if (EnableStopLoss)
					SetStopLoss(CalculationMode.Ticks, StopLossTicks);

				if (EnableProfitTarget)
					SetProfitTarget(CalculationMode.Ticks, ProfitTargetTicks);

				if (EnableTrailStop)
					SetTrailStop(CalculationMode.Ticks, TrailStopTicks);
			}
		}

		private void CheckForSellSignals(double currentPrice, double previousPrice)
		{
			// Don't enter new short if already short
			if (Position.MarketPosition == MarketPosition.Short)
				return;

			// Check for sell conditions based on level descriptions
			if (ShouldSellAtLevel(currentPrice, previousPrice))
			{
				EnterShort("Sell at Resistance/Pivot Bear");

				if (EnableStopLoss)
					SetStopLoss(CalculationMode.Ticks, StopLossTicks);

				if (EnableProfitTarget)
					SetProfitTarget(CalculationMode.Ticks, ProfitTargetTicks);

				if (EnableTrailStop)
					SetTrailStop(CalculationMode.Ticks, TrailStopTicks);
			}
		}

		private bool ShouldBuyAtLevel(double currentPrice, double previousPrice)
		{
			// Note: Since we cannot directly access the indicator's internal level data structure,
			// this is a template implementation. You will need to modify the PTZDailyPlanURLv2 indicator
			// to expose its price levels and descriptions through public properties or methods.

			// Template logic (requires indicator modification to expose level data):
			// foreach (var level in ptzIndicator.GetPriceLevels())
			// {
			//     string description = level.Description.ToLower();
			//     double levelPrice = level.Price;
			//
			//     bool isBuyLevel = false;
			//
			//     if (UseSupport && description.Contains(KeywordSupport.ToLower()))
			//         isBuyLevel = true;
			//     if (UsePivotBull && description.Contains(KeywordPivotBull.ToLower()))
			//         isBuyLevel = true;
			//     if (UseStrengthConfirmed && description.Contains(KeywordStrengthConfirmed.ToLower()))
			//         isBuyLevel = true;
			//
			//     if (isBuyLevel)
			//     {
			//         double proximity = PriceProximityTicks * TickSize;
			//
			//         if (TradeOnCrossover)
			//         {
			//             // Check if price crossed above the level
			//             if (previousPrice <= levelPrice && currentPrice > levelPrice)
			//                 return true;
			//         }
			//
			//         if (TradeOnTouch)
			//         {
			//             // Check if price is within proximity of the level
			//             if (Math.Abs(currentPrice - levelPrice) <= proximity)
			//                 return true;
			//         }
			//     }
			// }

			return false;
		}

		private bool ShouldSellAtLevel(double currentPrice, double previousPrice)
		{
			// Note: Since we cannot directly access the indicator's internal level data structure,
			// this is a template implementation. You will need to modify the PTZDailyPlanURLv2 indicator
			// to expose its price levels and descriptions through public properties or methods.

			// Template logic (requires indicator modification to expose level data):
			// foreach (var level in ptzIndicator.GetPriceLevels())
			// {
			//     string description = level.Description.ToLower();
			//     double levelPrice = level.Price;
			//
			//     bool isSellLevel = false;
			//
			//     if (UseResistance && description.Contains(KeywordResistance.ToLower()))
			//         isSellLevel = true;
			//     if (UsePivotBear && description.Contains(KeywordPivotBear.ToLower()))
			//         isSellLevel = true;
			//     if (UseWeaknessConfirmed && description.Contains(KeywordWeaknessConfirmed.ToLower()))
			//         isSellLevel = true;
			//
			//     if (isSellLevel)
			//     {
			//         double proximity = PriceProximityTicks * TickSize;
			//
			//         if (TradeOnCrossover)
			//         {
			//             // Check if price crossed below the level
			//             if (previousPrice >= levelPrice && currentPrice < levelPrice)
			//                 return true;
			//         }
			//
			//         if (TradeOnTouch)
			//         {
			//             // Check if price is within proximity of the level
			//             if (Math.Abs(currentPrice - levelPrice) <= proximity)
			//                 return true;
			//         }
			//     }
			// }

			return false;
		}

		#region Properties

		[NinjaScriptProperty]
		[Display(Name="Use Support Levels", Description="Enable trading at Support levels", Order=1, GroupName="1) Level Types")]
		public bool UseSupport
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Use Resistance Levels", Description="Enable trading at Resistance levels", Order=2, GroupName="1) Level Types")]
		public bool UseResistance
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Use Pivot Bull Levels", Description="Enable trading at Pivot Bull levels", Order=3, GroupName="1) Level Types")]
		public bool UsePivotBull
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Use Pivot Bear Levels", Description="Enable trading at Pivot Bear levels", Order=4, GroupName="1) Level Types")]
		public bool UsePivotBear
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Use Strength Confirmed", Description="Enable trading at Strength Confirmed levels", Order=5, GroupName="1) Level Types")]
		public bool UseStrengthConfirmed
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Use Weakness Confirmed", Description="Enable trading at Weakness Confirmed levels", Order=6, GroupName="1) Level Types")]
		public bool UseWeaknessConfirmed
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(Name="Price Proximity (Ticks)", Description="How close price must be to trigger (in ticks)", Order=1, GroupName="2) Entry Rules")]
		public int PriceProximityTicks
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Trade on Crossover", Description="Enter when price crosses the level", Order=2, GroupName="2) Entry Rules")]
		public bool TradeOnCrossover
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Trade on Touch", Description="Enter when price touches/is near the level", Order=3, GroupName="2) Entry Rules")]
		public bool TradeOnTouch
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Enable Stop Loss", Description="Use stop loss orders", Order=1, GroupName="3) Risk Management")]
		public bool EnableStopLoss
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Stop Loss (Ticks)", Description="Stop loss distance in ticks", Order=2, GroupName="3) Risk Management")]
		public int StopLossTicks
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Enable Profit Target", Description="Use profit target orders", Order=3, GroupName="3) Risk Management")]
		public bool EnableProfitTarget
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Profit Target (Ticks)", Description="Profit target distance in ticks", Order=4, GroupName="3) Risk Management")]
		public int ProfitTargetTicks
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Enable Trail Stop", Description="Use trailing stop orders", Order=5, GroupName="3) Risk Management")]
		public bool EnableTrailStop
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Trail Stop (Ticks)", Description="Trailing stop distance in ticks", Order=6, GroupName="3) Risk Management")]
		public int TrailStopTicks
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Support Keyword", Description="Keyword to identify support levels", Order=1, GroupName="4) Keywords")]
		public string KeywordSupport
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Resistance Keyword", Description="Keyword to identify resistance levels", Order=2, GroupName="4) Keywords")]
		public string KeywordResistance
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Pivot Bull Keyword", Description="Keyword to identify pivot bull levels", Order=3, GroupName="4) Keywords")]
		public string KeywordPivotBull
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Pivot Bear Keyword", Description="Keyword to identify pivot bear levels", Order=4, GroupName="4) Keywords")]
		public string KeywordPivotBear
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Strength Confirmed Keyword", Description="Keyword to identify strength confirmed levels", Order=5, GroupName="4) Keywords")]
		public string KeywordStrengthConfirmed
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Weakness Confirmed Keyword", Description="Keyword to identify weakness confirmed levels", Order=6, GroupName="4) Keywords")]
		public string KeywordWeaknessConfirmed
		{ get; set; }

		#endregion
	}
}
