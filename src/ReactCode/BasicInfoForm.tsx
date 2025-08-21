import React from "react";
import { YearLevelSelector } from "./YearLevelSelector";
import { weekDays } from "lib/constants";

interface BasicInfoFormProps {
  schoolName: string;
  setSchoolName: (name: string) => void;
  calendarYear: number;
  setCalendarYear: (year: number) => void;
  yearLevelsTaught: string[];
  setYearLevelsTaught: (levels: string[]) => void;
  workingDays: string[];
  setWorkingDays: (days: string[]) => void;
  onSubmit: (e: React.FormEvent) => void;
  loading: boolean;
  error: string | null;
}

export function BasicInfoForm({
  schoolName,
  setSchoolName,
  calendarYear,
  setCalendarYear,
  yearLevelsTaught,
  setYearLevelsTaught,
  workingDays,
  setWorkingDays,
  onSubmit,
  loading,
  error,
}: BasicInfoFormProps) {
  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!schoolName.trim()) {
      return;
    }
    onSubmit(e);
  };

  const handleWorkingDayToggle = (day: string) => {
    setWorkingDays(workingDays.includes(day) ? workingDays.filter((d) => d !== day) : [...workingDays, day]);
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-6">
      <div>
        <label htmlFor="schoolName" className="block text-sm font-medium text-gray-700">
          School Name
        </label>
        <input
          type="text"
          id="schoolName"
          name="schoolName"
          value={schoolName}
          onChange={(e) => setSchoolName(e.target.value)}
          className={`mt-1 block w-full rounded-md shadow-sm focus:ring-sage focus:border-sage sm:text-sm p-2
            ${error ? "border-red-300" : "border-gray-300"}`}
          required
          placeholder="Enter your school name"
          autoFocus
        />
        {error && <p className="mt-2 text-sm text-red-600">{error}</p>}
      </div>

      <div>
        <label htmlFor="calendarYear" className="block text-sm font-medium text-gray-700">
          Calendar Year
        </label>
        <select
          id="calendarYear"
          value={calendarYear}
          onChange={(e) => setCalendarYear(Number(e.target.value))}
          className="mt-1 block w-full rounded-md border-gray-300 p-2 shadow-sm focus:border-sage focus:ring-sage sm:text-sm">
          {[new Date().getFullYear(), new Date().getFullYear() + 1].map((year) => (
            <option key={year} value={year}>
              {year}
            </option>
          ))}
        </select>
      </div>

      <div>
        <label className="block text-sm font-medium text-gray-700 mb-2">Working Days</label>
        <div className="grid grid-cols-5 gap-3">
          {weekDays.map((day) => (
            <button
              key={day}
              type="button"
              onClick={() => handleWorkingDayToggle(day)}
              className={`
                ${workingDays.includes(day) ? "bg-sage text-white" : "bg-white text-gray-700 hover:bg-gray-50"}
                px-4 py-2 rounded-md text-sm font-medium border
                transition-colors duration-200
              `}>
              {day}
            </button>
          ))}
        </div>
      </div>

      <YearLevelSelector selectedYearLevels={yearLevelsTaught} onChange={setYearLevelsTaught} />

      <div className="flex justify-end">
        <button
          type="submit"
          disabled={loading || !schoolName.trim() || workingDays.length === 0}
          className="inline-flex justify-center rounded-md border border-transparent bg-sage py-2 px-4 text-sm font-medium text-white shadow-sm hover:bg-[#7A979B] disabled:opacity-50">
          Continue
        </button>
      </div>
    </form>
  );
}
