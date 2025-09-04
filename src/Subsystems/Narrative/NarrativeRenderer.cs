using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Wayfarer.Subsystems.NarrativeSubsystem
{
    /// <summary>
    /// Renders categorical templates into human-readable text
    /// This is the ONLY place where English text should be generated
    /// All text generation is rule-based from categorical properties
    /// </summary>
    public class NarrativeRenderer
    {
        private readonly Dictionary<string, Func<string, string>> _categoryRenderers;

        public NarrativeRenderer()
        {
            _categoryRenderers = new Dictionary<string, Func<string, string>>
            {
                ["greeting"] = RenderGreeting,
                ["emotional"] = RenderEmotional,
                ["gesture"] = RenderGesture,
                ["stance"] = RenderStance,
                ["urgency"] = RenderUrgency,
                ["activity"] = RenderActivity,
                ["posture"] = RenderPosture,
                ["behavior"] = RenderBehavior,
                ["query"] = RenderQuery,
                ["demand"] = RenderDemand,
                ["commitment"] = RenderCommitment,
                ["topic"] = RenderTopic,
                ["reaction"] = RenderReaction,
                ["response"] = RenderResponse,
                ["denial"] = RenderDenial,
                ["threat"] = RenderThreat,
                ["consequence"] = RenderConsequence,
                ["plea"] = RenderPlea,
                ["deadline"] = RenderDeadline,
                ["fate"] = RenderFate
            };
        }

        /// <summary>
        /// Convert categorical template to human-readable text
        /// </summary>
        public string RenderTemplate(string template)
        {
            if (string.IsNullOrEmpty(template))
                return "...";

            List<TemplatePart> parts = ParseTemplate(template);
            List<string> rendered = new List<string>();

            foreach (TemplatePart part in parts)
            {
                if (_categoryRenderers.ContainsKey(part.Category))
                {
                    string text = _categoryRenderers[part.Category](part.Value);
                    if (!string.IsNullOrEmpty(text))
                        rendered.Add(text);
                }
            }

            if (!rendered.Any())
                return "...";

            // Combine parts into natural sentence
            return CombineParts(rendered);
        }

        private List<TemplatePart> ParseTemplate(string template)
        {
            List<TemplatePart> parts = new List<TemplatePart>();
            string[] segments = template.Split(' ');

            foreach (string segment in segments)
            {
                int colonIndex = segment.IndexOf(':');
                if (colonIndex > 0)
                {
                    parts.Add(new TemplatePart
                    {
                        Category = segment.Substring(0, colonIndex),
                        Value = segment.Substring(colonIndex + 1)
                    });
                }
            }

            return parts;
        }

        private string CombineParts(List<string> parts)
        {
            if (parts.Count == 1)
                return parts[0].EndsWith("!") || parts[0].EndsWith("?") || parts[0].EndsWith(".")
                    ? parts[0]
                    : parts[0] + ".";

            // For desperate letter dialogue, combine with spaces
            string result = "";
            for (int i = 0; i < parts.Count; i++)
            {
                if (i == 0)
                {
                    result = parts[i];
                }
                else
                {
                    // Check if previous part ends with punctuation
                    if (result.EndsWith(".") || result.EndsWith("!") || result.EndsWith("?"))
                    {
                        result += " " + parts[i];
                    }
                    else
                    {
                        result += ". " + parts[i];
                    }
                }
            }

            return result;
        }

        private string CapitalizeFirst(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;
            return char.ToUpper(text[0]) + text.Substring(1);
        }

        // Category renderers - convert categorical values to text

        private string RenderGreeting(string value)
        {
            return value switch
            {
                "blessing" => "blessings upon you",
                "acknowledgment" => "ah, you're back",
                "commercial" => "good day for business",
                "formal" => "greetings",
                "simple" => "well met",
                "polite" => "good day",
                _ => "hello"
            };
        }

        private string RenderEmotional(string value)
        {
            return value switch
            {
                "desperate" => "desperation fills every word",
                "panicked" => "panic rises in their voice",
                "worried" => "worry creases their brow",
                "cautious" => "caution marks every movement",
                "stressed" => "stress shows in their bearing",
                "glad" => "gladness brightens their face",
                "trusting" => "trust shows in their eyes",
                "pleased" => "pleasure evident in their manner",
                "respectful" => "respect colors their tone",
                "honored" => "honor fills their bearing",
                "blessed" => "feeling blessed by your focus",
                "surprised" => "surprise crosses their features",
                "satisfied" => "satisfaction evident",
                "impressed" => "clearly impressed",
                "hopeful" => "hope kindles in their eyes",
                "intrigued" => "intrigue sharpens their gaze",
                "excited" => "excitement builds",
                "attentive" => "complete attention given",
                "energized" => "energy surges through them",
                "overwhelmed" => "overwhelm shows clearly",
                "processing" => "processing the information",
                "calculating" => "calculating rapidly",
                "composed" => "maintaining composure",
                "conflicted" => "conflict wars within",
                "neutral" => "steady and calm",
                _ => "their mood shifts"
            };
        }

        private string RenderGesture(string value)
        {
            return value switch
            {
                "clutching_letter" => "clutching a sealed letter",
                "white_knuckles" => "knuckles white with tension",
                "pacing" => "pacing restlessly",
                "sweating" => "sweat beading on their brow",
                "counting_coins" => "fingers moving as if counting coins",
                "ledger_clutching" => "gripping ledger pages",
                "whispering" => "voice dropping to whispers",
                "looking_around" => "eyes darting about nervously",
                "deference" => "bowing slightly",
                _ => "gesturing"
            };
        }

        private string RenderStance(string value)
        {
            return value switch
            {
                "withdrawn" => "withdrawn into themselves",
                "suspicious" => "suspicion in every line",
                "impatient" => "impatience radiating",
                "dismissive" => "dismissal in their bearing",
                "direct" => "direct and unflinching",
                "hostile" => "hostility crackling",
                "attention" => "standing at attention",
                _ => "standing"
            };
        }

        private string RenderUrgency(string value)
        {
            return value switch
            {
                "critical" => "time running out",
                "urgent" => "urgency pressing",
                "pressing" => "matter pressing",
                "moderate" => "steady need",
                "immediate_crisis" => "crisis upon us",
                "pressing_need" => "need pressing hard",
                "steady_concern" => "concern building",
                "manageable_pace" => "pace manageable",
                _ => $"{value} hours remain"
            };
        }

        private string RenderActivity(string value)
        {
            return value switch
            {
                "documenting" => "scratching notes",
                "arranging_goods" => "arranging wares",
                "polishing_glasses" => "polishing glass",
                "reviewing_correspondence" => "reading letters",
                "studying_texts" => "studying intently",
                "general" => "busy with tasks",
                _ => "working"
            };
        }

        private string RenderPosture(string value)
        {
            return value switch
            {
                "hunched_over" => "hunched over work",
                _ => "positioned"
            };
        }

        private string RenderBehavior(string value)
        {
            return value switch
            {
                "glancing_nervously" => "glancing about nervously",
                "glaring" => "glaring fiercely",
                "watchful" => "watching carefully",
                _ => "observing"
            };
        }

        private string RenderQuery(string value)
        {
            return value switch
            {
                "wellbeing" => "how fare you?",
                "focus_reason" => "what brings you here?",
                "purpose" => "what do you need?",
                "information_sought" => "what knowledge do you seek?",
                "business_interest" => "looking to trade?",
                "general" => "yes?",
                _ => "what?"
            };
        }

        private string RenderDemand(string value)
        {
            return value switch
            {
                "state_business" => "state your business",
                "time_is_money" => "time is coin",
                "plain_business" => "speak plainly",
                "immediate_departure" => "leave. Now",
                _ => "speak"
            };
        }

        private string RenderCommitment(string value)
        {
            return value switch
            {
                "assistance" => "I will help",
                "immediate" => "right away",
                _ => "I promise"
            };
        }

        private string RenderTopic(string value)
        {
            return value switch
            {
                "commerce" => "trade matters",
                _ => "this matter"
            };
        }

        private string RenderReaction(string value)
        {
            return value switch
            {
                "grateful" => "gratitude floods through",
                "surprised" => "surprise evident",
                "pleased" => "pleasure shows",
                "desperate_hope" => "desperate hope kindles",
                "engaged" => "engagement sharpens",
                _ => "reacting"
            };
        }

        private string RenderResponse(string value)
        {
            return value switch
            {
                "slight_relaxation" => "tension eases slightly",
                "engaged" => "leaning forward with interest",
                "generic" => "listening",
                _ => "responding"
            };
        }

        private string RenderDenial(string value)
        {
            return value switch
            {
                "no_understand" => "No, no, no... you don't understand!",
                _ => "no"
            };
        }

        private string RenderThreat(string value)
        {
            return value switch
            {
                "sunset_departure" => "Lord Blackwood leaves at sunset",
                _ => "danger looms"
            };
        }

        private string RenderConsequence(string value)
        {
            return value switch
            {
                "marriage_contract" => "If this letter doesn't reach him, my father will sign the marriage contract",
                _ => "terrible consequences"
            };
        }

        private string RenderPlea(string value)
        {
            return value switch
            {
                "begging" => "Please, I'm begging you!",
                _ => "please"
            };
        }

        private string RenderDeadline(string value)
        {
            return value switch
            {
                "tonight" => "tonight",
                _ => "soon"
            };
        }

        private string RenderFate(string value)
        {
            return value switch
            {
                "trapped_forever" => "I'll be trapped forever",
                _ => "doomed"
            };
        }

        private class TemplatePart
        {
            public string Category { get; set; }
            public string Value { get; set; }
        }
    }
}