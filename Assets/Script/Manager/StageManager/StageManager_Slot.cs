 // Start of Selection
 using System.Collections;
 using System.Collections.Generic;
 using DG.Tweening;
 using System.Linq;
 using UnityEngine;
 
 public partial class StageManager
 {
     private void SlotPosInit()
     {
         for (int i = 0; i < unitSlotList.Count; i++)
         {
             if (unitSlotList[i] != null)
             {
                 GameObject unit = unitSlotList[i].gameObject;
                 if (unit != null)
                 {
                     // 슬롯의 원래 위치를 저장합니다.
                     slotOriginalPositions[i] = unit.transform.position;
                 }
             }
         }
     }
     public void AllUnitBaseUpdate()
     {
         foreach (UnitSlotController unitSlot in unitSlotList)
         {
             unitSlot.unit.UnitBaseUpdate();
         }
         
         unitSlotList[currentTurnSlotNumber].unit.SetTurn(true);
     }
 
     /// <summary>
     /// 모든 유닛의 슬롯 위치를 이동시킵니다.
     /// </summary>
     public void MoveUnits()
     {
         for (int i = 0; i < unitSlotList.Count; i++)
         {
             GameObject moveSlot = unitSlotList[i].gameObject;
 
 
             Tween tween = DOTween.Sequence();
             tween = (moveSlot.transform.DOMove(slotOriginalPositions[i], 0.25f).SetEase(Ease.InOutQuad));
             tween.OnComplete(() =>
             {
                 moveSlot.transform.position = slotOriginalPositions[i];
             });
         }
     }
 
     /// <summary>
     /// 선택한 유닛을 오른쪽 또는 왼쪽으로 이동시킵니다.
     /// </summary>
     /// <param name="rightMove">오른쪽으로 이동하려면 true, 왼쪽으로 이동하려면 false.</param>
     public void DirectMoveUnit(bool rightMove)
     {
         if(currentPrograssState == ProgressState.UnitPlay || currentPrograssState == ProgressState.SkillTargetSearch)
         {
             UnitSlotController changeSlot = null;
 
             if (rightMove)
             {
                 if (currentTurnSlotNumber < unitSlotList.Count - 1)
                 {
                     changeSlot = unitSlotList[currentTurnSlotNumber];
                     unitSlotList[currentTurnSlotNumber] = unitSlotList[currentTurnSlotNumber + 1];
                     unitSlotList[currentTurnSlotNumber + 1] = changeSlot;
                     currentTurnSlotNumber = currentTurnSlotNumber + 1;
                 }
             }
             else
             {
                 if (currentTurnSlotNumber > 0)
                 {
                     changeSlot = unitSlotList[currentTurnSlotNumber];
                     unitSlotList[currentTurnSlotNumber] = unitSlotList[currentTurnSlotNumber - 1];
                     unitSlotList[currentTurnSlotNumber - 1] = changeSlot;
                     currentTurnSlotNumber = currentTurnSlotNumber - 1;
                 }
             }
 
             /*
             if (isUnitMoving || unitSlotList[currentTurnSlotNumber] == null || unitSlotList[moveTargetUnitNum] == null)
             {
                 Debug.LogWarning("Movement is currently locked or invalid slot numbers provided.");
                 return;
             }
             */

            AllUnitBaseUpdate();
             MoveUnits();
 
             for (int i = 0; i < unitSlotList.Count; i++)
             {
                 unitSlotList[i].slotGround.SetSlotGroundState(SlotGroundState.Default);
             }
             unitSlotList[currentTurnSlotNumber].slotGround.SetSlotGroundState(SlotGroundState.Select);
             currentPrograssState = ProgressState.UnitPlay;
         }
     }
 }
