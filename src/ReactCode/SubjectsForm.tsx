import React from "react";
import { getSubjectColor } from "lib/utils";

interface SubjectsFormProps {
  subjects: string[];
  selectedSubjects: string[];
  setSubjectsTaught: (subjects: string[]) => void;
  onBack: () => void;
  onSubmit: (e: React.FormEvent) => void;
  loading: boolean;
}

export function SubjectsForm({ subjects, selectedSubjects, setSubjectsTaught, onBack, onSubmit, loading }: SubjectsFormProps) {
  const handleToggle = (subject: string) => {
    setSubjectsTaught(selectedSubjects.includes(subject) ? selectedSubjects.filter((s) => s !== subject) : [...selectedSubjects, subject]);
  };

  return (
    <form onSubmit={onSubmit} className="space-y-6">
      <div>
        <label className="block text-sm font-medium text-gray-700 mb-2">Select the subjects you teach</label>
        <div className="grid grid-cols-2 gap-3">
          {subjects.map((subject) => (
            <button
              key={subject}
              type="button"
              onClick={() => handleToggle(subject)}
              className={`
                ${selectedSubjects.includes(subject) ? getSubjectColor(subject) : "bg-white text-gray-700 hover:bg-gray-50"}
                px-4 py-3 rounded-md text-sm font-medium 
                transition-colors duration-200
              `}>
              {subject}
            </button>
          ))}
        </div>
      </div>

      <div className="flex justify-between">
        <button
          type="button"
          onClick={onBack}
          className="inline-flex justify-center rounded-md border border-gray-300 bg-white py-2 px-4 text-sm font-medium text-gray-700 shadow-sm hover:bg-gray-50">
          Back
        </button>
        <button
          type="submit"
          disabled={loading}
          className="inline-flex justify-center rounded-md border border-transparent bg-sage py-2 px-4 text-sm font-medium text-white shadow-sm hover:bg-[#7A979B] disabled:opacity-50">
          Continue
        </button>
      </div>
    </form>
  );
}
