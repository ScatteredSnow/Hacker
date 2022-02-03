using UnityEngine;

namespace Hacker
{
    internal static class ESPUtils
    {
        static ESPUtils()
        {
            whiteTexture = Texture2D.whiteTexture;
            drawingTex = new Texture2D(1, 1);
            drawMaterial = new Material(Shader.Find("Hidden/Internal-Colored"))
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            drawMaterial.SetInt("_SrcBlend", 5);
            drawMaterial.SetInt("_DstBlend", 10);
            drawMaterial.SetInt("_Cull", 0);
            drawMaterial.SetInt("_ZWrite", 0);
        }

        public static Color GetHealthColour(float health, float maxHealth)
        {
            Color result = Color.green;
            float num = health / maxHealth;
            if (num >= 0.75f)
            {
                result = Color.green;
            }
            else
            {
                result = Color.yellow;
            }
            if (num <= 0.25f)
            {
                result = Color.red;
            }
            return result;
        }

        public static void DrawCircle(Color Col, Vector2 Center, float Radius)
        {
            GL.PushMatrix();
            if (!drawMaterial.SetPass(0))
            {
                GL.PopMatrix();
                return;
            }
            GL.Begin(1);
            GL.Color(Col);
            for (float num = 0f; num < 6.2831855f; num += 0.05f)
            {
                GL.Vertex(new Vector3(Mathf.Cos(num) * Radius + Center.x, Mathf.Sin(num) * Radius + Center.y));
                GL.Vertex(new Vector3(Mathf.Cos(num + 0.05f) * Radius + Center.x, Mathf.Sin(num + 0.05f) * Radius + Center.y));
            }
            GL.End();
            GL.PopMatrix();
        }

        public static void DrawLine(Vector2 start, Vector2 end, Color color, float width)
        {
            Color color2 = GUI.color;
            float num = (float)57.29577951308232;
            Vector2 vector = end - start;
            float num2 = num * Mathf.Atan(vector.y / vector.x);
            if (vector.x < 0f)
            {
                num2 += 180f;
            }
            int num3 = (int)Mathf.Ceil(width / 2f);
            GUIUtility.RotateAroundPivot(num2, start);
            GUI.color = color;
            GUI.DrawTexture(new Rect(start.x, start.y - (float)num3, vector.magnitude, width), Texture2D.whiteTexture, 0);
            GUIUtility.RotateAroundPivot(-num2, start);
            GUI.color = color2;
        }

        public static void OutlineBox(Vector2 pos, Vector2 size, Color colour)
        {
            Color color = GUI.color;
            GUI.color = colour;
            GUI.DrawTexture(new Rect(pos.x, pos.y, 1f, size.y), whiteTexture);
            GUI.DrawTexture(new Rect(pos.x + size.x, pos.y, 1f, size.y), whiteTexture);
            GUI.DrawTexture(new Rect(pos.x, pos.y, size.x, 1f), whiteTexture);
            GUI.DrawTexture(new Rect(pos.x, pos.y + size.y, size.x, 1f), whiteTexture);
            GUI.color = color;
        }

        public static bool IsOnScreen(Vector3 position)
        {
            return position.y > 0.01f && position.y < (float)Screen.height - 5f && position.z > 0.01f;
        }

        public static void CornerBox(Vector2 Head, float Width, float Height, float thickness, Color color, bool outline)
        {
            int num = (int)(Width / 4f);
            int num2 = num;
            if (outline)
            {
                RectFilled(Head.x - Width / 2f - 1f, Head.y - 1f, (float)(num + 2), 3f, Color.black);
                RectFilled(Head.x - Width / 2f - 1f, Head.y - 1f, 3f, (float)(num2 + 2), Color.black);
                RectFilled(Head.x + Width / 2f - (float)num - 1f, Head.y - 1f, (float)(num + 2), 3f, Color.black);
                RectFilled(Head.x + Width / 2f - 1f, Head.y - 1f, 3f, (float)(num2 + 2), Color.black);
                RectFilled(Head.x - Width / 2f - 1f, Head.y + Height - 4f, (float)(num + 2), 3f, Color.black);
                RectFilled(Head.x - Width / 2f - 1f, Head.y + Height - (float)num2 - 4f, 3f, (float)(num2 + 2), Color.black);
                RectFilled(Head.x + Width / 2f - (float)num - 1f, Head.y + Height - 4f, (float)(num + 2), 3f, Color.black);
                RectFilled(Head.x + Width / 2f - 1f, Head.y + Height - (float)num2 - 4f, 3f, (float)(num2 + 3), Color.black);
            }
            RectFilled(Head.x - Width / 2f, Head.y, (float)num, 1f, color);
            RectFilled(Head.x - Width / 2f, Head.y, 1f, (float)num2, color);
            RectFilled(Head.x + Width / 2f - (float)num, Head.y, (float)num, 1f, color);
            RectFilled(Head.x + Width / 2f, Head.y, 1f, (float)num2, color);
            RectFilled(Head.x - Width / 2f, Head.y + Height - 3f, (float)num, 1f, color);
            RectFilled(Head.x - Width / 2f, Head.y + Height - (float)num2 - 3f, 1f, (float)num2, color);
            RectFilled(Head.x + Width / 2f - (float)num, Head.y + Height - 3f, (float)num, 1f, color);
            RectFilled(Head.x + Width / 2f, Head.y + Height - (float)num2 - 3f, 1f, (float)(num2 + 1), color);
        }

        public static void RectFilled(float x, float y, float width, float height, Color color)
        {
            if (color != lastTexColour)
            {
                drawingTex.SetPixel(0, 0, color);
                drawingTex.Apply();
                lastTexColour = color;
            }
            GUI.DrawTexture(new Rect(x, y, width, height), drawingTex);
        }

        public static void DrawString(Vector2 pos, string text, Color color, bool center = true, int size = 12, FontStyle fontStyle = FontStyle.Bold, int depth = 1)
        {
            __style.fontSize = size;
            __style.richText = true;
            __style.normal.textColor = color;
            __style.fontStyle = fontStyle;
            __outlineStyle.fontSize = size;
            __outlineStyle.richText = true;
            __outlineStyle.normal.textColor = new Color(0f, 0f, 0f, 1f);
            __outlineStyle.fontStyle = fontStyle;
            GUIContent guicontent = new GUIContent(text);
            GUIContent guicontent2 = new GUIContent(text);
            if (center)
            {
                pos.x -= __style.CalcSize(guicontent).x / 2f;
            }
            switch (depth)
            {
                case 0:
                    GUI.Label(new Rect(pos.x, pos.y, 300f, 25f), guicontent, __style);
                    return;
                case 1:
                    GUI.Label(new Rect(pos.x + 1f, pos.y + 1f, 300f, 25f), guicontent2, __outlineStyle);
                    GUI.Label(new Rect(pos.x, pos.y, 300f, 25f), guicontent, __style);
                    return;
                case 2:
                    GUI.Label(new Rect(pos.x + 1f, pos.y + 1f, 300f, 25f), guicontent2, __outlineStyle);
                    GUI.Label(new Rect(pos.x - 1f, pos.y - 1f, 300f, 25f), guicontent2, __outlineStyle);
                    GUI.Label(new Rect(pos.x, pos.y, 300f, 25f), guicontent, __style);
                    return;
                case 3:
                    GUI.Label(new Rect(pos.x + 1f, pos.y + 1f, 300f, 25f), guicontent2, __outlineStyle);
                    GUI.Label(new Rect(pos.x - 1f, pos.y - 1f, 300f, 25f), guicontent2, __outlineStyle);
                    GUI.Label(new Rect(pos.x, pos.y - 1f, 300f, 25f), guicontent2, __outlineStyle);
                    GUI.Label(new Rect(pos.x, pos.y + 1f, 300f, 25f), guicontent2, __outlineStyle);
                    GUI.Label(new Rect(pos.x, pos.y, 300f, 25f), guicontent, __style);
                    return;
                default:
                    return;
            }
        }

        private static Texture2D drawingTex;

        private static Texture2D whiteTexture;

        private static Color lastTexColour;

        private static Material drawMaterial;

        private static GUIStyle __style = new GUIStyle();

        private static GUIStyle __outlineStyle = new GUIStyle();
    }
}
