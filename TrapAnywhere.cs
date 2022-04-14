using System.Collections.Generic;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("TrapAnywhere", "bmgjet", "0.0.1")]
    [Description("Lets you place traps anywhere")]
    class TrapAnywhere : RustPlugin
    {
        const string perm = "TrapAnywhere.use";
        private List<BaseEntity> traps = new List<BaseEntity>();

        void Init(){permission.RegisterPermission(perm, this);}

        private void OnPlayerInput(BasePlayer player, InputState input)
        {
            if (!permission.UserHasPermission(player.UserIDString, perm) || !input.WasJustPressed(BUTTON.FIRE_PRIMARY)){return;}
            var heldEntity = player.GetActiveItem();
            if (heldEntity == null){return;}
            if (heldEntity.skin != 0){return;}
            if (heldEntity.info.shortname == "trap.landmine" || heldEntity.info.shortname == "trap.bear")
            {
                string prefab = "assets/prefabs/deployable/landmine/landmine.prefab";
                if (heldEntity.info.shortname == "trap.bear") { prefab = "assets/prefabs/deployable/bear trap/beartrap.prefab"; }
                if (PlaceItem(player, prefab)){player.inventory.Take(null, heldEntity.info.itemid, 1);}
            }
        }

        private bool PlaceItem(BasePlayer player, string Selected)
        {
            RaycastHit rhit;
            if (!Physics.Raycast(player.eyes.HeadRay(), out rhit)){return false;}
            var newentity = GameManager.server.CreateEntity(Selected, rhit.point);
            if (newentity == null){return false;}
            newentity.transform.position = rhit.point;
            newentity.OwnerID = player.userID;
            newentity.Spawn();
            BaseEntity parent = rhit.GetEntity();
            if (parent != null) newentity.SetParent(parent,true,true);
            traps.Add(newentity);
            return true;
        }

        private void OnEntityBuilt(Planner plan, GameObject go)
        {
            if (go == null) { return; }
            BaseEntity entity = go.ToBaseEntity();
            if (entity == null){return;}
            BasePlayer player = BasePlayer.FindByID(entity.OwnerID);
            if (player == null){return;}
            timer.Once(0.1f,() =>
            {
                foreach (BaseEntity be in traps)
                {
                    if (be == null) continue;
                    if (entity?.Distance(be) < 1)
                    {
                        be.Kill();
                        traps.Remove(be);
                        return;
                    }
                }
            });
        }
    }
}