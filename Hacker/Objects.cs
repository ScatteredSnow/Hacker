using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hacker
{
    public class Objects : MonoBehaviour
    {
        public static List<EntityPlayer> PlayerList
        {
            get
            {
                if (GameManager.Instance != null && GameManager.Instance.World != null)
                {
                    return GameManager.Instance.World.GetPlayers();
                }
                return new List<EntityPlayer>();
            }
        }

        private void Start()
        {
            ZombieObj = new List<EntityZombie>();
            itemList = new List<EntityItem>();
            lastCachePlayer = Time.time + 5f;
            lastCacheZombies = Time.time + 3f;
            lastCacheItems = Time.time + 4f;
        }

        private void Update()
        {
            if (gameManager == null)
            {
                gameManager = GameManager.Instance;
            }
            if (questJournal == null && PlayerLocal != null)
            {
                questJournal = PlayerLocal.QuestJournal;
            }
            if (waypointCollection == null && PlayerLocal != null)
            {
                waypointCollection = PlayerLocal.Waypoints;
            }
            if (Time.time >= lastCacheItems)
            {
                ZombieObj = FindObjectsOfType<EntityZombie>().ToList<EntityZombie>();
                //itemList = FindObjectsOfType<EntityItem>().ToList<EntityItem>();
                lastCacheItems = Time.time + 4f;
            }
            if (PlayerLocal == null || Time.time >= lastCachePlayer)
            {
                PlayerLocal = FindObjectOfType<EntityPlayerLocal>();
                lastCachePlayer = Time.time + 5f;
            }
        }

        public static GameManager gameManager;

        public static EntityPlayerLocal PlayerLocal;

        public static QuestJournal questJournal;

        public static List<EntityItem> itemList;

        public static List<EntityZombie> ZombieObj;

        public static WaypointCollection waypointCollection;

        private float lastCachePlayer;

        private float lastCacheZombies;

        private float lastCacheItems;
    }
}
