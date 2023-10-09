using UnityEngine;

namespace LS
{
    public static class ExtensionMethods
    {
        public static Color WithAlpha(this Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }
    }
}