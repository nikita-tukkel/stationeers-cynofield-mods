using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Electrical;
using HarmonyLib;
using System;
using TMPro;
using UnityEngine;

namespace cynofield.mods
{
    public class ThingsUi
    {
        public bool Supports(Thing thing)
        {
            return true;
        }

        public void RenderArAnnotation(Thing thing, Canvas canvas, TextMeshProUGUI textMesh)
        {
            // TODO different types of rendering complexity
            var desc = AugmentedReality.Instance.thingsUi.Description2d(thing);
            textMesh.text = desc;
            //             $@"{thing.DisplayName}
            // {desc}
            // please <color=red><b>don't</b></color> play with me";
        }

        public string Description2d(Thing thing)
        {
            return Describe(thing);
        }

        public void Destroy()
        {

        }

        public string Describe(Thing thing)
        {
            if (thing == null)
                return "nothing";
            switch (thing)
            {
                case Cable obj:
                    var net = obj.CableNetwork;
                    return $"network: {net.DisplayName} {PowerDisplay(net.CurrentLoad)} / {PowerDisplay(net.PotentialLoad)}";
                case Transformer obj:
                    {
                        var color = obj.Powered ? "green" : "red";
                        return
    $@"{obj.DisplayName}
<color={color}><b>{obj.Setting}</b></color>
{PowerDisplay(obj.UsedPower)}
{PowerDisplay(obj.AvailablePower)}";
                    }
                case CircuitHousing obj:
                    {
                        var chip = obj._ProgrammableChipSlot.Occupant as ProgrammableChip;
                        if (chip == null)
                        {
                            return $@"{obj.DisplayName}
<color=red><b>db={obj.Setting}</b>
no chip</color>";
                        }
                        else
                        {
                            var registers = Traverse.Create(chip)
                            .Field("_Registers").GetValue() as double[];
                            return
    $@"{obj.DisplayName}
<color=green><b>db={obj.Setting}</b><mspace=1em> </mspace>r15={registers[15]}</color>
<mspace=0.65em>{DisplayRegisters(registers)}</mspace>
";
                        }
                    }
                default:
                    return thing.ToString();
            }
        }

        private string DisplayRegisters(double[] registers)
        {
            string result = "";
            int count = 0;
            for (int i = 0; i < 16; i++)
            {
                if (registers[i] == 0)
                    continue;
                count++;
                result += $"r{i}={Math.Round(registers[i], 2)}";
                if (count > 0 && count % 2 == 0)
                    result += "\n";
                else
                    result += "<mspace=1em> </mspace>";
            }
            return result;
        }

        static private string PowerDisplay(float power)
        {
            if (power > 900_000)
            {
                return $"{Math.Round(power / 1_000_000f, 2)}MW";
            }
            else if (power > 900)
            {
                return $"{Math.Round(power / 1_000f, 2)}kW";
            }

            else
            {
                return $"{Math.Round(power, 2)}W";
            }
        }
    }
}
