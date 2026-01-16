using System;
using System.Drawing;
using Client.MirObjects;
using Client.MirScenes;

namespace Client.LocalGame
{
    /// <summary>
    /// 本地模式移动处理器
    /// 处理本地游戏中的玩家移动、转向等动作
    /// </summary>
    public static class LocalMovementHandler
    {
        /// <summary>
        /// 处理转向动作
        /// </summary>
        public static void HandleTurn(UserObject user, MirDirection direction)
        {
            if (user == null) return;

            user.Direction = direction;
            MapControl.NextAction = CMain.Time + 2500;
            GameScene.CanRun = false;

            Console.WriteLine($"[本地模式] 转向: {direction}");
        }

        /// <summary>
        /// 处理行走动作
        /// </summary>
        public static void HandleWalk(UserObject user, MirDirection direction)
        {
            if (user == null) return;

            // 计算新位置
            Point newLocation = Functions.PointMove(user.CurrentLocation, direction, 1);

            // 检查新位置是否有效
            if (!CanMoveTo(user, newLocation))
            {
                Console.WriteLine("[本地模式] 无法移动到该位置");
                return;
            }

            // 更新玩家位置和方向
            user.Direction = direction;
            user.CurrentLocation = newLocation;
            user.MapLocation = newLocation;

            // 更新游戏状态
            GameScene.LastRunTime = CMain.Time;
            if (GameScene.Scene?.MapControl != null)
            {
                GameScene.Scene.MapControl.FloorValid = false;
            }
            GameScene.CanRun = true;
            MapControl.NextAction = CMain.Time + 2500;

            Console.WriteLine($"[本地模式] 走到: {newLocation}");
        }

        /// <summary>
        /// 处理跑步动作
        /// </summary>
        public static void HandleRun(UserObject user, MirDirection direction)
        {
            if (user == null) return;

            // 计算新位置（跑步走2格）
            Point newLocation = Functions.PointMove(user.CurrentLocation, direction, 2);

            // 检查新位置是否有效
            if (!CanMoveTo(user, newLocation))
            {
                // 如果跑不到，尝试走1格
                newLocation = Functions.PointMove(user.CurrentLocation, direction, 1);
                if (!CanMoveTo(user, newLocation))
                {
                    Console.WriteLine("[本地模式] 无法移动到该位置");
                    return;
                }
            }

            // 更新玩家位置和方向
            user.Direction = direction;
            user.CurrentLocation = newLocation;
            user.MapLocation = newLocation;

            // 更新游戏状态
            GameScene.LastRunTime = CMain.Time;
            if (GameScene.Scene?.MapControl != null)
            {
                GameScene.Scene.MapControl.FloorValid = false;
            }
            MapControl.NextAction = CMain.Time + (user.Sprint ? 1000 : 2500);

            Console.WriteLine($"[本地模式] 跑到: {newLocation}");
        }

        /// <summary>
        /// 检查是否可以移动到指定位置
        /// </summary>
        private static bool CanMoveTo(UserObject user, Point location)
        {
            try
            {
                var mapControl = GameScene.Scene?.MapControl;
                if (mapControl == null) return false;

                // 检查地图边界
                if (location.X < 0 || location.X >= mapControl.Width ||
                    location.Y < 0 || location.Y >= mapControl.Height)
                {
                    return false;
                }

                // 检查是否有阻挡
                if (location.X < 0 || location.Y < 0 ||
                    location.X >= mapControl.M2CellInfo.GetLength(0) ||
                    location.Y >= mapControl.M2CellInfo.GetLength(1))
                {
                    return false;
                }

                var cell = mapControl.M2CellInfo[location.X, location.Y];
                if (cell == null) return false;

                // 检查是否有障碍物
                // TODO: 添加更详细的碰撞检测

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LocalMovementHandler] 检查移动失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 处理推挤动作
        /// </summary>
        public static void HandlePush(UserObject user, Point newLocation)
        {
            if (user == null) return;

            // 检查新位置是否有效
            if (!CanMoveTo(user, newLocation))
            {
                return;
            }

            // 更新玩家位置
            user.CurrentLocation = newLocation;
            user.MapLocation = newLocation;

            // 更新游戏状态
            GameScene.LastRunTime = CMain.Time;
            if (GameScene.Scene?.MapControl != null)
            {
                GameScene.Scene.MapControl.FloorValid = false;
            }
            MapControl.InputDelay = CMain.Time + 500;
            GameScene.CanRun = false;
            GameScene.CanMove = false;

            Console.WriteLine($"[本地模式] 被推到: {newLocation}");
        }
    }
}
