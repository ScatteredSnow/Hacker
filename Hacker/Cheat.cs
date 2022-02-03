using System;
using UnityEngine;

namespace Hacker
{
    public class Cheat : MonoBehaviour
    {
        private void Start()
        {
            lastChamTime = Time.time + 10f;
            chamsMaterial = new Material(Shader.Find("Hidden/Internal-Colored"))
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            _Color = Shader.PropertyToID("_Color");
            chamsMaterial.SetInt("_SrcBlend", 5);
            chamsMaterial.SetInt("_DstBlend", 10);
            chamsMaterial.SetInt("_Cull", 0);
            chamsMaterial.SetInt("_ZTest", 8);
            chamsMaterial.SetInt("_ZWrite", 0);
            chamsMaterial.SetColor(_Color, Color.magenta);
        }

        private void Aimbot()
        {
            float num = 9999f;
            Vector2 vector = Vector2.zero;
            foreach (EntityZombie entityZombie in Objects.ZombieObj)
            {
                if (entityZombie && entityZombie.IsAlive())
                {
                    //entityZombie.VisiblityCheck();
                    Vector3 bellyPosition = entityZombie.emodel.GetBellyPosition();
                    Vector3 vector2 = ESP.mainCam.WorldToScreenPoint(bellyPosition);
                    if (Vector2.Distance(new Vector2((float)(Screen.width / 2), (float)(Screen.height / 2)), new Vector2(vector2.x, (float)Screen.height - vector2.y)) <= 150f && ESPUtils.IsOnScreen(vector2))
                    {
                        float num2 = Math.Abs(Vector2.Distance(new Vector2(vector2.x, (float)Screen.height - vector2.y), new Vector2((float)(Screen.width / 2), (float)(Screen.height / 2))));
                        if (num2 < num)
                        {
                            num = num2;
                            vector = new Vector2(vector2.x, (float)Screen.height - vector2.y);
                        }
                    }
                }
            }
            //if (Objects.PlayerList.Count > 1)
            //{
            //	foreach (EntityPlayer entityPlayer in Objects.PlayerList)
            //	{
            //		if (entityPlayer && entityPlayer.IsAlive() && entityPlayer != Objects.PlayerLocal)
            //		{
            //			Vector3 bellyPosition2 = entityPlayer.emodel.GetBellyPosition();
            //			Vector3 vector3 = ESP.mainCam.WorldToScreenPoint(bellyPosition2);
            //			if (Vector2.Distance(new Vector2((float)(Screen.width / 2), (float)(Screen.height / 2)), new Vector2(vector3.x, vector3.y)) <= 150f && ESPUtils.IsOnScreen(vector3))
            //			{
            //				float num3 = Math.Abs(Vector2.Distance(new Vector2(vector3.x, (float)Screen.height - vector3.y), new Vector2((float)(Screen.width / 2), (float)(Screen.height / 2))));
            //				if (num3 < num)
            //				{
            //					num = num3;
            //					vector = new Vector2(vector3.x, (float)Screen.height - vector3.y);
            //				}
            //			}
            //		}
            //	}
            //}
            if (vector != Vector2.zero)
            {
                double num4 = (double)(vector.x - (float)Screen.width / 2f);
                double num5 = (double)(vector.y - (float)Screen.height / 2f);
                num4 /= 10.0;
                num5 /= 10.0;
                //mouse_event(1, (int)num4, (int)num5, 0, 0);
            }
        }

        private void MagicBullet()
        {
            EntityZombie entityZombie = null;
            EntityPlayer entityPlayer = null;
            if (Objects.ZombieObj.Count == 0) return;

            foreach (EntityZombie entityZombie2 in Objects.ZombieObj)
            {
                if (entityZombie2 && entityZombie2.IsAlive())
                {
                    Vector3 position = entityZombie2.emodel.GetHeadTransform().position;
                    Vector3 vector = ESP.mainCam.WorldToScreenPoint(position);
                    if (Vector2.Distance(new Vector2((float)(Screen.width / 2), (float)(Screen.height / 2)), new Vector2(vector.x, vector.y)) <= 120f)
                    {
                        entityZombie = entityZombie2;
                    }
                }
            }
            foreach (EntityPlayer entityPlayer2 in Objects.PlayerList)
            {
                if (entityPlayer2 && entityPlayer2.IsAlive() && entityPlayer2 != Objects.PlayerLocal)
                {
                    Vector3 position2 = entityPlayer2.emodel.GetHeadTransform().position;
                    Vector3 vector2 = ESP.mainCam.WorldToScreenPoint(position2);
                    if (Vector2.Distance(new Vector2((float)(Screen.width / 2), (float)(Screen.height / 2)), new Vector2(vector2.x, vector2.y)) <= 120f)
                    {
                        entityPlayer = entityPlayer2;
                    }
                }
            }
            if (entityPlayer)
            {
                DamageSource damageSource = new DamageSource(0, EnumDamageTypes.Concuss);
                entityZombie.DamageEntity(damageSource, 100, false, 1f);
                entityZombie.AwardKill(Objects.PlayerLocal);
            }
            if (entityZombie)
            {
                entityZombie.DamageEntity(new DamageSource(0, EnumDamageTypes.Concuss)
                {
                    CreatorEntityId = Objects.PlayerLocal.entityId
                }, 100, false, 1f);
                entityZombie.AwardKill(Objects.PlayerLocal);
                Objects.PlayerLocal.AddKillXP(entityZombie, 1f);
            }
        }

        private void Update()
        {
            if (noWeaponBob && Objects.PlayerLocal)
            {
                vp_FPWeapon vp_FPWeapon = Objects.PlayerLocal.vp_FPWeapon;
                if (vp_FPWeapon)
                {
                    vp_FPWeapon.BobRate = Vector4.zero;
                    vp_FPWeapon.ShakeAmplitude = Vector3.zero;
                    vp_FPWeapon.RenderingFieldOfView = 120f;
                    vp_FPWeapon.StepForceScale = 0f;
                }
            }
            if (Input.GetKeyDown(KeyCode.PageDown))
            {
                if (!Objects.PlayerLocal)
                {
                    return;
                }
                Inventory inventory = Objects.PlayerLocal.inventory;
                if (inventory != null)
                {
                    ItemActionAttack holdingGun = inventory.GetHoldingGun();
                    if (holdingGun != null)
                    {
                        holdingGun.InfiniteAmmo = !holdingGun.InfiniteAmmo;
                    }
                }
            }
            if (Input.GetKey(KeyCode.LeftAlt) && magicBullet)
            {
                MagicBullet();
            }
            if (Input.GetKeyDown(KeyCode.F2))
            {
                speed = !speed;
                Time.timeScale = (speed ? 6f : 1f);
            }
            if (Time.time >= lastChamTime && chams)
            {
                foreach (Entity entity in FindObjectsOfType<Entity>())
                {
                    if (entity)
                    {
                        switch (entity.entityType)
                        {
                            case EntityType.Unknown:
                                ApplyChams(entity, Color.white);
                                break;
                            case EntityType.Player:
                                ApplyChams(entity, Color.cyan);
                                break;
                            case EntityType.Zombie:
                                ApplyChams(entity, Color.red);
                                break;
                            case EntityType.Animal:
                                ApplyChams(entity, Color.yellow);
                                break;
                        }
                    }
                }
                lastChamTime = Time.time + 10f;
            }
        }

        private void ApplyChams(Entity entity, Color color)
        {
            foreach (Renderer renderer in entity.GetComponentsInChildren<Renderer>())
            {
                renderer.material = chamsMaterial;
                renderer.material.SetColor(_Color, color);
            }
        }

        private int _Color;

        private float lastChamTime;

        private Material chamsMaterial;

        public static bool speed;

        public static bool aimbot;

        public static bool infiniteAmmo;

        public static bool noWeaponBob;

        public static bool magicBullet;

        public static bool chams;
    }
}
