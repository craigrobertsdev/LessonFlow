import { yearLevels } from "lib/constants";

interface YearLevelSelectorProps {
  selectedYearLevels: string[];
  onChange: (yearLevels: string[]) => void;
}

export function YearLevelSelector({ selectedYearLevels, onChange }: YearLevelSelectorProps) {
  const handleToggle = (yearLevel: string) => {
    onChange(selectedYearLevels.includes(yearLevel) ? selectedYearLevels.filter((yl) => yl !== yearLevel) : [...selectedYearLevels, yearLevel]);
  };

  return (
    <div>
      <label className="block text-sm font-medium text-gray-700 mb-2">Year Levels</label>
      <div className="grid grid-cols-3 gap-3">
        {yearLevels.map((yearLevel) => (
          <button
            key={yearLevel}
            type="button"
            onClick={() => handleToggle(yearLevel)}
            className={`
              ${selectedYearLevels.includes(yearLevel) ? "bg-sage text-white" : "bg-white text-gray-700 hover:bg-gray-50"}
              px-3 py-2 rounded-md text-sm font-medium border focus:outline-none
            `}>
            {yearLevel}
          </button>
        ))}
      </div>
    </div>
  );
}
