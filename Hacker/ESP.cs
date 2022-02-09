using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Hacker
{
    public class ESP : MonoBehaviour
    {
        [DllImport("user32.dll")] private static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

        private void Start()
        {
            mainCam = Camera.main;
            blackCol = new Color(0f, 0f, 0f, 120f);
            entityBoxCol = new Color(0.42f, 0.36f, 0.9f, 1f);
            crosshairCol = new Color32(30, 144, byte.MaxValue, byte.MaxValue);
        }

        private void Update()
        {
            if (zombieCornerBox)
            {
                zombieBox = false;
            }
            else if (zombieBox && zombieCornerBox)
            {
                zombieCornerBox = false;
            }
            if (playerCornerBox)
            {
                playerBox = false;
            }
            else if (playerBox && playerCornerBox)
            {
                playerCornerBox = false;
            }
            if (Objects.PlayerLocal)
            {
                Objects.PlayerLocal.weaponCrossHairAlpha = (crosshair ? 0f : 255f);
            }
        }
        private double GetY(Vector3 vector)
        {
            vector.y -= 1.6f;//修正高度
            return Math.Atan2(-vector.y, Math.Sqrt(vector.x * vector.x + vector.z * vector.z)) * 180f / Math.PI;
        }
        private double GetX(Vector3 vector)
        {
            if (vector.x == 0 && vector.z < 0) return 180f; //S
            if (vector.x == 0 && vector.z > 0) return 0f;//N
            if (vector.x > 0 && vector.z == 0) return 90f;//E
            if (vector.x < 0 && vector.z == 0) return 270f;//W
            if (vector.x > 0 && vector.z > 0)//1
            {
                return Math.Atan2(vector.x, vector.z) * 180f / Math.PI;
            }
            if (vector.x < 0 && vector.z > 0)//2
            {
                return Math.Atan2(vector.x, vector.z) * 180f / Math.PI;
            }
            if (vector.x < 0 && vector.z < 0)//3
            {
                return Math.Atan2(vector.x, vector.z) * 180f / Math.PI;
            }
            if (vector.x > 0 && vector.z < 0)//4
            {
                return Math.Atan2(vector.x, vector.z) * 180f / Math.PI;
            }
            return 0f;
        }

        private void OnGUI()
        {
            if (Event.current.type != EventType.Repaint)
            {
                return;
            }
            if (!mainCam)
            {
                mainCam = Camera.main;
            }
            if (fovCircle)
            {
                ESPUtils.DrawCircle(Color.black, new Vector2((float)(Screen.width / 2), (float)(Screen.height / 2)), 149f);
                ESPUtils.DrawCircle(Color.black, new Vector2((float)(Screen.width / 2), (float)(Screen.height / 2)), 151f);
                ESPUtils.DrawCircle(new Color32(30, 144, byte.MaxValue, byte.MaxValue), new Vector2((float)(Screen.width / 2), (float)(Screen.height / 2)), 150f);
            }
            if (crosshair)
            {
                Vector2 start = new Vector2((float)(Screen.width / 2) - crosshairScale, (float)(Screen.height / 2));
                Vector2 end = new Vector2((float)(Screen.width / 2) + crosshairScale, (float)(Screen.height / 2));
                Vector2 start2 = new Vector2((float)(Screen.width / 2), (float)(Screen.height / 2) - crosshairScale);
                Vector2 end2 = new Vector2((float)(Screen.width / 2), (float)(Screen.height / 2) + crosshairScale);
                ESPUtils.DrawLine(start, end, crosshairCol, lineThickness);
                ESPUtils.DrawLine(start2, end2, crosshairCol, lineThickness);
            }
            if (Objects.ZombieObj.Count > 0 && (zombieName || zombieBox || zombieHealth))
            {
                float lastDis = 9999f;
                EntityZombie LockentityZombie = null;
                foreach (EntityZombie entityZombie in Objects.ZombieObj)
                {
                    if (entityZombie && entityZombie.IsAlive())
                    {
                        Vector3 vector = mainCam.WorldToScreenPoint(entityZombie.transform.position);
                        if (ESPUtils.IsOnScreen(vector))
                        {
                            Vector3 vector2 = mainCam.WorldToScreenPoint(entityZombie.emodel.GetHeadTransform().position);
                            float num = Mathf.Abs(vector2.y - vector.y);
                            float num2 = vector.x - num * 0.3f;
                            float num3 = (float)Screen.height - vector2.y;
                            if (zombieBox)
                            {
                                ESPUtils.OutlineBox(new Vector2(num2 - 1f, num3 - 1f), new Vector2(num / 2f + 2f, num + 2f), blackCol);
                                ESPUtils.OutlineBox(new Vector2(num2, num3), new Vector2(num / 2f, num), entityBoxCol);
                                ESPUtils.OutlineBox(new Vector2(num2 + 1f, num3 + 1f), new Vector2(num / 2f - 2f, num - 2f), blackCol);
                            }
                            else if (zombieCornerBox)
                            {
                                ESPUtils.CornerBox(new Vector2(vector2.x, num3), num / 2f, num, 2f, entityBoxCol, true);
                            }
                            if (zombieName)
                            {
                                ESPUtils.DrawString(new Vector2(vector.x, (float)Screen.height - vector.y + 8f), entityZombie.EntityName.Replace("zombie", "Zombie_"), Color.red, true, 12, 0, 1);
                            }
                            if (zombieHealth)
                            {
                                float num4 = (float)entityZombie.Health;
                                int maxHealth = entityZombie.GetMaxHealth();
                                float num5 = num4 / (float)maxHealth;
                                float num6 = num * num5;
                                Color healthColour = ESPUtils.GetHealthColour(num4, (float)maxHealth);
                                ESPUtils.RectFilled(num2 - 5f, num3, 4f, num, blackCol);
                                ESPUtils.RectFilled(num2 - 4f, num3 + num - num6 - 1f, 2f, num6, healthColour);
                            }
                            if (Vector2.Distance(new Vector2((float)(Screen.width / 2), (float)(Screen.height / 2)), new Vector2(vector2.x, num3)) <= 150f)
                            {
                                float Dis = Math.Abs(Vector2.Distance(new Vector2(vector2.x, (float)Screen.height - vector2.y), new Vector2((float)(Screen.width / 2), (float)(Screen.height / 2))));
                                if (Dis < lastDis)
                                {
                                    lastDis = Dis;
                                    LockentityZombie = entityZombie;
                                }
                            }
                        }
                    }
                    //LockVector.x = LockVector.x - (float)Screen.width / 2f;
                    //LockVector.y = LockVector.y + (float)Screen.height / 2f;
                    //LockVector.x /= 10;
                    //LockVector.y /= 10;
                    if (Input.GetKey(KeyCode.LeftAlt) && Cheat.aimbot)
                    {
                        if (LockentityZombie != null)
                        {
                            //Vector3 LockVector = mainCam.WorldToScreenPoint(LockentityZombie.emodel.GetHeadTransform().position);
                            Vector3 LockVector = LockentityZombie.emodel.GetHeadTransform().position - Objects.PlayerLocal.transform.position;
                            Objects.PlayerLocal.vp_FPCamera.Yaw = (float)GetX(LockVector);//((float)GetX(LockVector) - Objects.PlayerLocal.vp_FPCamera.Yaw) / 10;
                            Objects.PlayerLocal.vp_FPCamera.Pitch = (float)GetY(LockVector);//((float)GetY(LockVector) - Objects.PlayerLocal.vp_FPCamera.Pitch) / 10;
                            Debug.Log($"{LockVector}{Objects.PlayerLocal.vp_FPCamera.Pitch}:{Objects.PlayerLocal.vp_FPCamera.Yaw}");
                        }
                        //mouse_event(1, (int)LockVector.x, (int)LockVector.y, 0, 0);
                    }
                }
            }
            if (Objects.PlayerList.Count > 1 && (playerName || playerBox || playerHealth))
            {
                foreach (EntityPlayer entityPlayer in Objects.PlayerList)
                {
                    if (entityPlayer && !(entityPlayer == Objects.PlayerLocal) && entityPlayer.IsAlive())
                    {
                        Vector3 vector3 = mainCam.WorldToScreenPoint(entityPlayer.transform.position);
                        if (ESPUtils.IsOnScreen(vector3))
                        {
                            Vector3 vector4 = mainCam.WorldToScreenPoint(entityPlayer.emodel.GetHeadTransform().position);
                            float num7 = Mathf.Abs(vector4.y - vector3.y);
                            float num8 = vector3.x - num7 * 0.3f;
                            float num9 = (float)Screen.height - vector4.y;
                            if (playerBox)
                            {
                                ESPUtils.OutlineBox(new Vector2(num8 - 1f, num9 - 1f), new Vector2(num7 / 2f + 2f, num7 + 2f), blackCol);
                                ESPUtils.OutlineBox(new Vector2(num8, num9), new Vector2(num7 / 2f, num7), entityBoxCol);
                                ESPUtils.OutlineBox(new Vector2(num8 + 1f, num9 + 1f), new Vector2(num7 / 2f - 2f, num7 - 2f), blackCol);
                            }
                            else if (playerCornerBox)
                            {
                                ESPUtils.CornerBox(new Vector2(vector4.x, num9), num7 / 2f, num7, 2f, entityBoxCol, true);
                            }
                            if (playerName)
                            {
                                ESPUtils.DrawString(new Vector2(vector3.x, (float)Screen.height - vector3.y + 8f), entityPlayer.EntityName, Color.red, true, 12, 0, 1);
                            }
                            if (playerHealth)
                            {
                                float num10 = (float)entityPlayer.Health;
                                int maxHealth2 = entityPlayer.GetMaxHealth();
                                float num11 = num10 / (float)maxHealth2;
                                float num12 = num7 * num11;
                                Color healthColour2 = ESPUtils.GetHealthColour(num10, (float)maxHealth2);
                                ESPUtils.RectFilled(num8 - 5f, num9, 4f, num7, blackCol);
                                ESPUtils.RectFilled(num8 - 4f, num9 + num7 - num12 - 1f, 2f, num12, healthColour2);
                            }
                        }
                    }
                }
            }
        }

        public static Camera mainCam;

        private Color blackCol;

        private Color entityBoxCol;

        private Color crosshairCol;

        private readonly float crosshairScale = 14f;

        private readonly float lineThickness = 1.75f;

        public static bool playerBox = true;

        public static bool playerName = true;

        public static bool playerHealth = true;

        public static bool playerCornerBox = false;

        public static bool zombieBox = true;

        public static bool zombieName = true;

        public static bool zombieHealth = true;

        public static bool zombieCornerBox = false;

        public static bool crosshair = false;

        public static bool fovCircle = false;
    }
}
