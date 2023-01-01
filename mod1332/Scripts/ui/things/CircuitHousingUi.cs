using System;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Electrical;
using HarmonyLib;

namespace cynofield.mods.ui.things
{
    class CircuitHousingUi : IThingArDescriber
    {
        Type IThingArDescriber.SupportedType() { return typeof(CircuitHousing); }

        string IThingArDescriber.Describe(Thing thing)
        {
            var obj = thing as CircuitHousing;
            var chip = obj._ProgrammableChipSlot.Occupant as ProgrammableChip;
            if (chip == null)
            {
                return
$@"{obj.DisplayName}
<color=red><b>db={obj.Setting}</b>
no chip</color>";
            }
            else
            {
                var registers = GetRegisters(chip);
                return
$@"{obj.DisplayName}
<color=green><b>db={obj.Setting}</b><mspace=1em> </mspace>r15={registers[15]}</color>
{DisplayRegisters(registers)}
";
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

        private double[] GetRegisters(ProgrammableChip chip)
        {
            return Traverse.Create(chip).Field("_Registers").GetValue() as double[];
        }

        private readonly UiUtils utils = new UiUtils();
    }
}
