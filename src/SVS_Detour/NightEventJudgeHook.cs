using BepInEx.Unity.IL2CPP.Hook;
using HarmonyLib;
using Il2CppInterop.Common;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppInterop.Runtime.Runtime;
using System;
using System.Runtime.InteropServices;
using SV;
using SaveData;

namespace SVS_Detour
{
    internal static unsafe class NightEventJudgeHook
    {
        private static INativeDetour _detour;
        private static JudgeDelegate _orig;

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        private delegate ulong JudgeDelegate(IntPtr self, IntPtr active, IntPtr passive);

        private static Actor _nightChara;
        private static int _charaID = -1;
        private static int _nightType = 0;
        private static float _nightWeight = 0;
        private static bool _disableEvent = false;

        public static void Install()
        {
            var method = AccessTools.Method(typeof(NightEventManager), "Judge", new[] { typeof(Actor), typeof(Actor) });
            if (method == null)
                throw new Exception("NightEventManager.Judge not found");

            var ptrField = Il2CppInteropUtils.GetIl2CppMethodInfoPointerFieldForGeneratedMethod(method);
            if (ptrField == null)
                throw new Exception($"Couldn't obtain NativeMethodInfoPtr field for {method}");

            var rawMethodInfoPtr = (IntPtr)(ptrField.GetValue(null) ?? IntPtr.Zero);
            if (rawMethodInfoPtr == IntPtr.Zero)
                throw new Exception($"NativeMethodInfoPtr for {method} is null");

            unsafe
            {
                var nativeMethodInfo =
                    UnityVersionHandler.Wrap((Il2CppMethodInfo*)(void*)rawMethodInfoPtr)
                    ?? throw new Exception($"Wrapped method info for {method} is invalid");

                IntPtr methodPtr = nativeMethodInfo.MethodPointer;

                //DetourPlugin.Log.LogInfo($"Judge methodPtr: 0x{methodPtr.ToInt64():X}");

                _detour = INativeDetour.CreateAndApply(
                    methodPtr,
                    (JudgeDelegate)JudgeDetour,
                    out _orig
                );
            }

            //Log.LogInfo($"NightEventManager.Judge Detour created. Orig delegate null? {_orig == null}");
        }

        private static ulong JudgeDetour(IntPtr self, IntPtr active, IntPtr passive)
        {
            NightEventManager instance = null;
            Actor activeActor = null;
            Actor passiveActor = null;

            try
            {
                if (self != IntPtr.Zero)
                    instance = new Il2CppObjectBase(self).Cast<NightEventManager>();

                if (active != IntPtr.Zero)
                    activeActor = new Il2CppObjectBase(active).Cast<Actor>();

                if (passive != IntPtr.Zero)
                    passiveActor = new Il2CppObjectBase(passive).Cast<Actor>();
            }
            catch (Exception ex)
            {
                //DetourPlugin.Log.LogError($"Wrap failed: {ex}");
            }

            ulong raw = _orig(self, active, passive);

            int resultId = (int)(raw & 0xFFFFFFFF);
            int floatBits = (int)(raw >> 32);
            float weight = BitConverter.Int32BitsToSingle(floatBits);

            var result = Judge_Postfix(activeActor, passiveActor, resultId, weight);

            raw = Pack(result.Item1, result.Item2);

            return raw;
        }

        private static ulong Pack(int item1, float item2)
        {
            uint lo = unchecked((uint)item1);
            uint hi = unchecked((uint)BitConverter.SingleToInt32Bits(item2));
            return ((ulong)hi << 32) | lo;
        }

        private static bool ShouldModify(Actor active, Actor passive, int id, float weight)
        {
            return id != -1 && active != null && passive != null;
        }

        private static (int, float) Judge_Postfix(Actor active, Actor passive, int resultId, float weight)
        {
            int activeID = active.charasGameParam.Index;
            int passiveID = passive.charasGameParam.Index;

            //Log original values
            if (DetourPlugin.GetShowLog()) DetourPlugin.Log.LogInfo($"Judge chara ID({active.charasGameParam.Index}, {passive.charasGameParam.Index}) -> id={resultId}, weight={weight}");

            //Conditional change example
            if (ShouldModify(active, passive, resultId, weight))
            {
                if (_charaID > -1)
                {
                    if (active.charasGameParam.Index == _charaID)
                    {
                        resultId = _nightType;
                        weight = _nightWeight;
                    }
                    else
                    {
                        resultId = 0;
                        weight = 0;
                    }
                        
                }
                else
                {
                    resultId = 0;
                    weight = 0;
                }

                if (resultId == 0) weight = 0f;
                else weight *= 100f;

                //Optional clamp if needed
                if (weight > 500000f) weight = 500000f;
                if (weight < 0f) weight = 0f;

                //Log changed value
                if (DetourPlugin.GetShowLog()) DetourPlugin.Log.LogInfo($"Judge: chara ID: {active.charasGameParam.Index}, PC ID: {passive.charasGameParam.Index} modified -> id={resultId}, weight={weight}");
            }

            return (resultId, weight);
        }

        public static void SetNightParameters(int charaID, int type, float weight)
        {
            _charaID = charaID;
            _nightType = type;
            _nightWeight = weight;
        }
    }

    public static class NightEvent
    {
        public static void SetNightEventCharacter(int charaID, int type, float weight)
        {
            NightEventJudgeHook.SetNightParameters(charaID, type, weight);
        }
    }
}
