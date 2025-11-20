using System.Collections.Generic;
using System.Linq;

namespace AD_BulkChanges.Models
{
    public enum LogicOperator
    {
        AND,
        OR
    }

    public class FilterCondition
    {
        public string Field { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public LogicOperator Operator { get; set; } = LogicOperator.AND;

        public override string ToString()
        {
            return $"{Field} = '{Value}'";
        }
    }

    public class TargetFieldChange
    {
        public string Field { get; set; } = string.Empty;
        public string NewValue { get; set; } = string.Empty;

        public override string ToString()
        {
            return $"{Field} = '{NewValue}'";
        }
    }

    public class FieldMapping
    {
        public string OldValue { get; set; } = string.Empty;
        public string NewValue { get; set; } = string.Empty;
        
        // Erweiterte Filter-Bedingungen mit Operatoren
        public List<FilterCondition> Conditions { get; set; } = new List<FilterCondition>();
        
        // Mehrere Ziel-Felder
        public List<TargetFieldChange> TargetChanges { get; set; } = new List<TargetFieldChange>();
        
        // Gibt an ob erweiterte Bedingungen verwendet werden
        public bool UseAdvancedFilters => Conditions.Count > 0;
        
        public override string ToString()
        {
            if (UseAdvancedFilters)
            {
                var conditionStrings = new List<string>();
                for (int i = 0; i < Conditions.Count; i++)
                {
                    if (i > 0)
                    {
                        conditionStrings.Add(Conditions[i].Operator == LogicOperator.AND ? "UND" : "ODER");
                    }
                    conditionStrings.Add(Conditions[i].ToString());
                }

                var changes = TargetChanges.Count > 0 
                    ? string.Join(", ", TargetChanges) 
                    : $"'{NewValue}'";

                return $"Wenn ({string.Join(" ", conditionStrings)}) → {changes}";
            }
            return $"{OldValue} → {NewValue}";
        }
    }
}
