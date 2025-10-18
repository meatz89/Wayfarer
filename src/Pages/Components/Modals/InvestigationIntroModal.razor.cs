using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace Wayfarer.Pages.Components
{
    public class InvestigationIntroModalBase : ComponentBase
    {
        [Parameter] public InvestigationIntroResult Data { get; set; }
        [Parameter] public EventCallback OnBegin { get; set; }
        [Parameter] public EventCallback OnDismiss { get; set; }

        protected async Task BeginInvestigation()
        {
            await OnBegin.InvokeAsync();
        }

        protected async Task DismissModal()
        {
            await OnDismiss.InvokeAsync();
        }

        protected string GetHeaderColor()
        {
            if (Data == null)
                throw new InvalidOperationException("Data parameter is required");

            if (string.IsNullOrEmpty(Data.ColorCode))
                return "#7a8b5a";
            return Data.ColorCode;
        }

        protected string GetHeaderColorLight()
        {
            if (Data == null)
                throw new InvalidOperationException("Data parameter is required");

            // Lighten the color for gradient
            if (string.IsNullOrEmpty(Data.ColorCode))
                return "#9eb87a";

            // Simple lightening by adding 20 to each RGB component
            string hex = Data.ColorCode.TrimStart('#');
            if (hex.Length == 6)
            {
                int r = System.Convert.ToInt32(hex.Substring(0, 2), 16);
                int g = System.Convert.ToInt32(hex.Substring(2, 2), 16);
                int b = System.Convert.ToInt32(hex.Substring(4, 2), 16);

                r = System.Math.Min(255, r + 32);
                g = System.Math.Min(255, g + 32);
                b = System.Math.Min(255, b + 32);

                return $"#{r:X2}{g:X2}{b:X2}";
            }

            return "#9eb87a";
        }

        protected string GetBorderColor()
        {
            if (Data == null)
                throw new InvalidOperationException("Data parameter is required");

            // Darken the color for border
            if (string.IsNullOrEmpty(Data.ColorCode))
                return "#5c7a4a";

            string hex = Data.ColorCode.TrimStart('#');
            if (hex.Length == 6)
            {
                int r = System.Convert.ToInt32(hex.Substring(0, 2), 16);
                int g = System.Convert.ToInt32(hex.Substring(2, 2), 16);
                int b = System.Convert.ToInt32(hex.Substring(4, 2), 16);

                r = System.Math.Max(0, r - 32);
                g = System.Math.Max(0, g - 32);
                b = System.Math.Max(0, b - 32);

                return $"#{r:X2}{g:X2}{b:X2}";
            }

            return "#5c7a4a";
        }
    }
}
