
using SpinOfFortune.Shared;

namespace SpinOfFortune.Table;

public class PayTableRule
{
    public int Pay { get; set; }
    public bool IsMultiplier { get; set; }
    public int Multiplier { get; set; } = 1;
    public CombinationPayType CombinationPayType { get; set; }

    private PayTableRule() {}

    public PayTableRule(bool isMultiplier, CombinationPayType combinationPayType)
    {
        Pay = (int)combinationPayType;
        IsMultiplier = isMultiplier;
        CombinationPayType = combinationPayType;
    }

    public static PayTableRule Copy(PayTableRule rule)
    {
        return new PayTableRule
        {
            Pay = rule.Pay,
            IsMultiplier = rule.IsMultiplier,
            Multiplier = rule.Multiplier,
            CombinationPayType = rule.CombinationPayType,
        };
    }
}

public class PayTable
{
    private readonly Dictionary<(SymbolType, SymbolType, SymbolType), PayTableRule> rules;

    public PayTable()
    {
        rules = [];
        InitPayTable();
    }

    public void InitPayTable()
    {
        // 2x 5x 2x, 2x 4x 2x, 2x 3x 2x, 2x 2x 2x
        {
            PayTableRule pay5x = new(false, CombinationPayType.Wild2x5x2x);
            AddRuleWithWildCount(pay5x, 3,
                    [SymbolType.Wild2x],
                    [SymbolType.Wild5x],
                    [SymbolType.Wild2x]);
            PayTableRule pay4x = new(false, CombinationPayType.Wild2x4x2x);
            AddRuleWithWildCount(pay4x, 3,
                    [SymbolType.Wild2x],
                    [SymbolType.Wild4x],
                    [SymbolType.Wild2x]);
            PayTableRule pay3x = new(false, CombinationPayType.Wild2x3x2x);
            AddRuleWithWildCount(pay3x, 3,
                    [SymbolType.Wild2x],
                    [SymbolType.Wild3x],
                    [SymbolType.Wild2x]);
            PayTableRule pay2x = new(false, CombinationPayType.Wild2x2x2x);
            AddRuleWithWildCount(pay2x, 3,
                    [SymbolType.Wild2x],
                    [SymbolType.Wild2x],
                    [SymbolType.Wild2x]);
        }

        // 7 7 7
        {
            PayTableRule pay = new(true, CombinationPayType.Three7);
            AddRules(pay,
                    [SymbolType.Seven, SymbolType.Wild2x],
                    [SymbolType.Seven, SymbolType.Wild5x, SymbolType.Wild4x, SymbolType.Wild3x, SymbolType.Wild2x],
                    [SymbolType.Seven, SymbolType.Wild2x]);
        }

        // 7B 7B 7B
        {
            PayTableRule pay = new(true, CombinationPayType.Three7Bar);
            AddRules(pay,
                    [SymbolType.SevenBar, SymbolType.Wild2x],
                    [SymbolType.SevenBar, SymbolType.Wild5x, SymbolType.Wild4x, SymbolType.Wild3x, SymbolType.Wild2x],
                    [SymbolType.SevenBar, SymbolType.Wild2x]);
        }

        // 3B 3B 3B
        {
            PayTableRule pay = new(true, CombinationPayType.Three3Bar);
            AddRules(pay,
                    [SymbolType.ThreeBar, SymbolType.Wild2x],
                    [SymbolType.ThreeBar, SymbolType.Wild5x, SymbolType.Wild4x, SymbolType.Wild3x, SymbolType.Wild2x],
                    [SymbolType.ThreeBar, SymbolType.Wild2x]);
        }

        // Any Three 7
        {
            PayTableRule pay = new(true, CombinationPayType.AnyThree7);
            AddRules(pay,
                    [SymbolType.Seven, SymbolType.SevenBar, SymbolType.Wild2x],
                    [SymbolType.Seven, SymbolType.SevenBar, SymbolType.Wild5x, SymbolType.Wild4x, SymbolType.Wild3x, SymbolType.Wild2x],
                    [SymbolType.Seven, SymbolType.SevenBar, SymbolType.Wild2x]);
        }

        // 2B 2B 2B
        {
            PayTableRule pay = new(true, CombinationPayType.Three2Bar);
            AddRules(pay,
                    [SymbolType.TwoBar, SymbolType.Wild2x],
                    [SymbolType.TwoBar, SymbolType.Wild5x, SymbolType.Wild4x, SymbolType.Wild3x, SymbolType.Wild2x],
                    [SymbolType.TwoBar, SymbolType.Wild2x]);
        }

        // One 2x One 5x
        {
            PayTableRule pay = new(false, CombinationPayType.One2xOne5x);
            AddRuleWithWildCount(pay, 2,
                    [SymbolType.Wild2x, SymbolType.Blank, SymbolType.Bonus],
                    [SymbolType.Wild5x],
                    [SymbolType.Wild2x, SymbolType.Blank, SymbolType.Bonus]);
        }

        // One 2x, One 4x
        {
            PayTableRule pay = new(false, CombinationPayType.One2xOne4x);
            AddRuleWithWildCount(pay, 2,
                    [SymbolType.Wild2x, SymbolType.Blank, SymbolType.Bonus],
                    [SymbolType.Wild4x],
                    [SymbolType.Wild2x, SymbolType.Blank, SymbolType.Bonus]);
        }

        // One 2x, One 3x
        {
            PayTableRule pay = new(false, CombinationPayType.One2xOne3x);
            AddRuleWithWildCount(pay, 2,
                    [SymbolType.Wild2x, SymbolType.Blank, SymbolType.Bonus],
                    [SymbolType.Wild3x],
                    [SymbolType.Wild2x, SymbolType.Blank, SymbolType.Bonus]);
        }

        // 1B 1B 1B
        {
            PayTableRule pay = new(true, CombinationPayType.Three1Bar);
            AddRules(pay,
                    [SymbolType.OneBar, SymbolType.Wild2x],
                    [SymbolType.OneBar, SymbolType.Wild5x, SymbolType.Wild4x, SymbolType.Wild3x, SymbolType.Wild2x],
                    [SymbolType.OneBar, SymbolType.Wild2x]);
        }

        // Two 2x
        {
            PayTableRule pay = new(false, CombinationPayType.Two2x);
            AddRuleWithWildCount(pay, 2,
                    [SymbolType.Wild2x, SymbolType.Blank, SymbolType.Bonus],
                    [SymbolType.Wild2x, SymbolType.Blank, SymbolType.Bonus],
                    [SymbolType.Wild2x, SymbolType.Blank, SymbolType.Bonus]);
        }

        // Any Three Bar
        {
            PayTableRule pay = new(true, CombinationPayType.AnyThreeBar);
            AddRules(pay,
                    [SymbolType.OneBar, SymbolType.TwoBar, SymbolType.ThreeBar, SymbolType.SevenBar, SymbolType.Wild2x],
                    [SymbolType.OneBar, SymbolType.TwoBar, SymbolType.ThreeBar, SymbolType.SevenBar, SymbolType.Wild5x, SymbolType.Wild4x, SymbolType.Wild3x, SymbolType.Wild2x],
                    [SymbolType.OneBar, SymbolType.TwoBar, SymbolType.ThreeBar, SymbolType.SevenBar, SymbolType.Wild2x]);
        }

        // Any One 5x
        {
            PayTableRule pay = new(false, CombinationPayType.AnyOne5x);
            AddRuleWithWildCount(pay, 1,
                    [SymbolType.Seven, SymbolType.OneBar, SymbolType.TwoBar, SymbolType.ThreeBar, SymbolType.SevenBar, SymbolType.Blank, SymbolType.Bonus],
                    [SymbolType.Wild5x],
                    [SymbolType.Seven, SymbolType.OneBar, SymbolType.TwoBar, SymbolType.ThreeBar, SymbolType.SevenBar, SymbolType.Blank, SymbolType.Bonus]);
        }

        // Any One 4x
        {
            PayTableRule pay = new(false, CombinationPayType.AnyOne4x);
            AddRuleWithWildCount(pay, 1,
                    [SymbolType.Seven, SymbolType.OneBar, SymbolType.TwoBar, SymbolType.ThreeBar, SymbolType.SevenBar, SymbolType.Blank, SymbolType.Bonus],
                    [SymbolType.Wild4x],
                    [SymbolType.Seven, SymbolType.OneBar, SymbolType.TwoBar, SymbolType.ThreeBar, SymbolType.SevenBar, SymbolType.Blank, SymbolType.Bonus]);
        }

        // Any One 3x
        {
            PayTableRule pay = new(false, CombinationPayType.AnyOne3x);
            AddRuleWithWildCount(pay, 1,
                    [SymbolType.Seven, SymbolType.OneBar, SymbolType.TwoBar, SymbolType.ThreeBar, SymbolType.SevenBar, SymbolType.Blank, SymbolType.Bonus],
                    [SymbolType.Wild3x],
                    [SymbolType.Seven, SymbolType.OneBar, SymbolType.TwoBar, SymbolType.ThreeBar, SymbolType.SevenBar, SymbolType.Blank, SymbolType.Bonus]);
        }

        // Any One 2x
        {
            PayTableRule pay = new(false, CombinationPayType.AnyOne2x);
            AddRuleWithWildCount(pay, 1,
                    [SymbolType.Wild2x, SymbolType.Seven, SymbolType.OneBar, SymbolType.TwoBar, SymbolType.ThreeBar, SymbolType.SevenBar, SymbolType.Blank, SymbolType.Bonus],
                    [SymbolType.Wild2x, SymbolType.Seven, SymbolType.OneBar, SymbolType.TwoBar, SymbolType.ThreeBar, SymbolType.SevenBar, SymbolType.Blank, SymbolType.Bonus],
                    [SymbolType.Wild2x, SymbolType.Seven, SymbolType.OneBar, SymbolType.TwoBar, SymbolType.ThreeBar, SymbolType.SevenBar, SymbolType.Blank, SymbolType.Bonus]);
        }
    }

    public void OutputRules()
    {
        foreach (var rule in rules)
        {
            Console.WriteLine($"{GetSymbolTypeString(rule.Key.Item1)} {GetSymbolTypeString(rule.Key.Item2)} {GetSymbolTypeString(rule.Key.Item3)}" +
                $" : {rule.Value.Pay * rule.Value.Multiplier}");
        }

        Console.WriteLine();
        Console.WriteLine($"Total Rules: {rules.Count}");
    }

    public static string GetSymbolTypeString(SymbolType symbol) =>
        symbol switch
        {
            SymbolType.Wild5x => "5x",
            SymbolType.Wild4x => "4x",
            SymbolType.Wild3x => "3x",
            SymbolType.Wild2x => "2x",
            SymbolType.Seven => "7",
            SymbolType.SevenBar => "7B",
            SymbolType.OneBar => "1B",
            SymbolType.TwoBar => "2B",
            SymbolType.ThreeBar => "3B",
            SymbolType.Blank => "-",
            SymbolType.Bonus => "BW",
            _ => "Unknown"
        };

    private void AddRules(PayTableRule rule, ReadOnlySpan<SymbolType> reels1, ReadOnlySpan<SymbolType> reels2, ReadOnlySpan<SymbolType> reels3)
    {
        for (int i = 0; i < reels1.Length; i++)
        {
            for (int j = 0; j < reels2.Length; j++)
            {
                for (int k = 0; k < reels3.Length; k++)
                {
                    var key = (reels1[i], reels2[j], reels3[k]);
                    if (rules.ContainsKey(key))
                    {
                        continue;
                    }

                    if (rule.IsMultiplier)
                    {
                        rule.Multiplier = GetWildMultiplier(key.Item1) * GetWildMultiplier(key.Item2) * GetWildMultiplier(key.Item3);
                    }

                    rules[key] = PayTableRule.Copy(rule);
                }
            }
        }
    }

    private void AddRuleWithWildCount(PayTableRule rule, int wildCount, ReadOnlySpan<SymbolType> reels1, ReadOnlySpan<SymbolType> reels2, ReadOnlySpan<SymbolType> reels3)
    {
        for (int i = 0; i < reels1.Length; i++)
        {
            for (int j = 0; j < reels2.Length; j++)
            {
                for (int k = 0; k < reels3.Length; k++)
                {
                    var key = (reels1[i], reels2[j], reels3[k]);
                    if (GetWildCount(key.Item1) + GetWildCount(key.Item2) + GetWildCount(key.Item3) < wildCount)
                    {
                        continue;
                    }

                    if (rules.ContainsKey(key))
                    {
                        continue;
                    }
                    if (rule.IsMultiplier)
                    {
                        rule.Multiplier = GetWildMultiplier(key.Item1) * GetWildMultiplier(key.Item2) * GetWildMultiplier(key.Item3);
                    }
                    rules[key] = PayTableRule.Copy(rule);
                }
            }
        }
    }

    private static int GetWildCount(SymbolType symbol) =>
        symbol switch
        {
            SymbolType.Wild5x => 1,
            SymbolType.Wild4x => 1,
            SymbolType.Wild3x => 1,
            SymbolType.Wild2x => 1,
            _ => 0
        };

    private static int GetWildMultiplier(SymbolType wild) =>
        wild switch
        {
            SymbolType.Wild5x => 5,
            SymbolType.Wild4x => 4,
            SymbolType.Wild3x => 3,
            SymbolType.Wild2x => 2,
            _ => 1
        };

    public (int pay, CombinationPayType combinationPayType) GetPay(ReadOnlySpan<SymbolType> symbols)
    {
        var key = (symbols[0], symbols[1], symbols[2]);
        if (!rules.TryGetValue(key, out var rule))
        {
            return (0, CombinationPayType.None);
        }

        return (rule.Pay * rule.Multiplier, rule.CombinationPayType);
    }
}