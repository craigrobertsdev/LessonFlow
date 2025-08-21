import React, { useState } from "react";
import { LoadingSpinner } from "../LoadingSpinner";

interface TimingFormProps {
  scheduleSlots: ScheduleSlot[];
  onBack: () => void;
  onSubmit: (scheduleConfig: ScheduleConfig) => void;
  loading: boolean;
}

// A slot represents either a lesson or a break in the day
interface ScheduleSlot {
  id: string;
  isBreak: boolean;
  name: string;
  startTime: string;
  endTime: string;
}

export interface ScheduleConfig {
  numberOfLessons: number;
  numberOfBreaks: number;
  startTime: string;
  endTime: string;
  slots: ScheduleSlot[];
}

export function TimingForm({ scheduleSlots, onBack, onSubmit, loading }: TimingFormProps) {
  // Default school day times
  const [schoolDayStart, setSchoolDayStart] = useState<string>(scheduleSlots[0].startTime);
  const [schoolDayEnd, setSchoolDayEnd] = useState<string>(scheduleSlots[scheduleSlots.length - 1].endTime);

  // Slots containing lessons and breaks in order
  const [slots, setSlots] = useState<ScheduleSlot[]>(scheduleSlots);

  // Validation
  const [validationError, setValidationError] = useState<string | null>(null);

  // Validate the schedule for logical time sequence
  const validateSchedule = (): boolean => {
    // Reset validation
    setValidationError(null);

    // Check for empty slots
    if (slots.length === 0) {
      setValidationError("You need at least one slot in your schedule");
      return false;
    }

    // Convert school day times to minutes for comparison
    const schoolStartMinutes = timeToMinutes(schoolDayStart);
    const schoolEndMinutes = timeToMinutes(schoolDayEnd);

    if (schoolStartMinutes >= schoolEndMinutes) {
      setValidationError("School day end time must be after start time");
      return false;
    }

    // Check if first slot starts before or at school start time
    const firstSlotStartMinutes = timeToMinutes(slots[0].startTime);
    if (firstSlotStartMinutes < schoolStartMinutes) {
      setValidationError("First slot cannot start before school day begins");
      return false;
    }

    // Check if last slot ends after or at school end time
    const lastSlotEndMinutes = timeToMinutes(slots[slots.length - 1].endTime);
    if (lastSlotEndMinutes > schoolEndMinutes) {
      setValidationError("Last slot cannot end after school day ends");
      return false;
    }

    // Check for overlapping slots and logical sequence
    for (let i = 0; i < slots.length; i++) {
      const currentSlot = slots[i];
      const currentStartMinutes = timeToMinutes(currentSlot.startTime);
      const currentEndMinutes = timeToMinutes(currentSlot.endTime);

      // Start time must be before end time
      if (currentStartMinutes >= currentEndMinutes) {
        setValidationError(`${currentSlot.name}: End time must be after start time`);
        return false;
      }

      // Check if slot duration is too short (less than 5 minutes)
      if (currentEndMinutes - currentStartMinutes < 5) {
        setValidationError(`${currentSlot.name}: Duration must be at least 5 minutes`);
        return false;
      }

      // Check for continuity with next slot if there is one
      if (i < slots.length - 1) {
        const nextSlot = slots[i + 1];
        const nextStartMinutes = timeToMinutes(nextSlot.startTime);

        if (currentEndMinutes !== nextStartMinutes) {
          setValidationError(`Gap or overlap between ${currentSlot.name} and ${nextSlot.name}`);
          return false;
        }
      }
    }

    // Check if there are any lessons
    const hasLessons = slots.some((slot) => !slot.isBreak);
    if (!hasLessons) {
      setValidationError("You must have at least one lesson in your schedule");
      return false;
    }

    return true;
  };

  // Convert HH:MM time string to minutes since midnight
  const timeToMinutes = (time: string): number => {
    const [hours, minutes] = time.split(":").map(Number);
    return hours * 60 + minutes;
  };

  // Convert minutes since midnight to HH:MM format
  const minutesToTime = (minutes: number): string => {
    const hours = Math.floor(minutes / 60);
    const mins = minutes % 60;
    return `${hours.toString().padStart(2, "0")}:${mins.toString().padStart(2, "0")}`;
  };

  // Add a new slot to the schedule
  const addSlot = (type: "lesson" | "break") => {
    // If there are no slots yet, start from school day start
    let startTime = schoolDayStart;

    // Otherwise, use the end time of the last slot
    if (slots.length > 0) {
      startTime = slots[slots.length - 1].endTime;
    }

    // Calculate a default end time (30 minutes later for lessons, 15 for breaks)
    const startMinutes = timeToMinutes(startTime);
    const endMinutes = startMinutes + (type === "lesson" ? 30 : 15);

    // Don't exceed school day end
    const schoolEndMinutes = timeToMinutes(schoolDayEnd);
    const endTime = minutesToTime(Math.min(endMinutes, schoolEndMinutes));

    // Generate name based on type and count
    const lessonCount = slots.filter((s) => !s.isBreak).length + 1;
    const breakCount = slots.length - lessonCount + 1;

    let name = type === "lesson" ? `Lesson ${lessonCount}` : `Break ${breakCount}`;

    // Special names for first two breaks if they are the first of their kind
    if (type === "break" && breakCount === 1) {
      name = "Recess";
    } else if (type === "break" && breakCount === 2) {
      name = "Lunch";
    }

    // Create new slot
    const newSlot: ScheduleSlot = {
      id: Date.now().toString(),
      isBreak: type === "break",
      name,
      startTime,
      endTime,
    };

    setSlots([...slots, newSlot]);
  };

  // Remove a slot from the schedule
  const removeSlot = (id: string) => {
    // Find the slot to remove
    const index = slots.findIndex((slot) => slot.id === id);

    if (index === -1) return;

    // If not the last slot, we need to adjust the next slot's start time
    if (index < slots.length - 1) {
      const removedSlot = slots[index];
      const nextSlot = slots[index + 1];

      // Copy all slots
      const newSlots = [...slots];

      // Adjust the next slot to start at the same time as the removed slot
      newSlots[index + 1] = {
        ...nextSlot,
        startTime: removedSlot.startTime,
      };

      // Remove the slot
      newSlots.splice(index, 1);
      setSlots(newSlots);
    } else {
      // If it's the last slot, just remove it
      setSlots(slots.filter((slot) => slot.id !== id));
    }
  };

  // Update slot fields
  const updateSlot = (id: string, field: keyof ScheduleSlot, value: string) => {
    const index = slots.findIndex((slot) => slot.id === id);

    if (index === -1) return;

    const updatedSlots = [...slots];
    updatedSlots[index] = {
      ...updatedSlots[index],
      [field]: value,
    };

    // If changing start or end time, we need to adjust adjacent slots
    if (field === "startTime") {
      // If not the first slot, update the previous slot's end time
      if (index > 0) {
        updatedSlots[index - 1] = {
          ...updatedSlots[index - 1],
          endTime: value,
        };
      }
    } else if (field === "endTime") {
      // If not the last slot, update the next slot's start time
      if (index < slots.length - 1) {
        updatedSlots[index + 1] = {
          ...updatedSlots[index + 1],
          startTime: value,
        };
      }
    }

    setSlots(updatedSlots);
  };

  // Move a slot up in the order
  const moveSlotUp = (id: string) => {
    const index = slots.findIndex((slot) => slot.id === id);

    if (index <= 0) return;

    // Swap the slots but maintain start/end time continuity
    const updatedSlots = [...slots];
    const movingSlot = { ...updatedSlots[index] };
    const previousSlot = { ...updatedSlots[index - 1] };

    // Calculate duration of each slot
    const movingSlotDuration = timeToMinutes(movingSlot.endTime) - timeToMinutes(movingSlot.startTime);
    const previousSlotDuration = timeToMinutes(previousSlot.endTime) - timeToMinutes(previousSlot.startTime);

    // Swap positions while maintaining durations
    const newStartTime = previousSlot.startTime;
    const newMiddleTime = minutesToTime(timeToMinutes(newStartTime) + movingSlotDuration);
    const newEndTime = minutesToTime(timeToMinutes(newMiddleTime) + previousSlotDuration);

    // Update the slots
    updatedSlots[index - 1] = {
      ...movingSlot,
      startTime: newStartTime,
      endTime: newMiddleTime,
    };

    updatedSlots[index] = {
      ...previousSlot,
      startTime: newMiddleTime,
      endTime: newEndTime,
    };

    // If this is not the last slot, update the next slot's start time
    if (index + 1 < updatedSlots.length) {
      updatedSlots[index + 1] = {
        ...updatedSlots[index + 1],
        startTime: newEndTime,
      };
    }

    setSlots(updatedSlots);
  };

  // Move a slot down in the order
  const moveSlotDown = (id: string) => {
    const index = slots.findIndex((slot) => slot.id === id);

    if (index === -1 || index >= slots.length - 1) return;

    // Just use moveSlotUp on the next slot
    moveSlotUp(slots[index + 1].id);
  };

  // Handle form submission
  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();

    if (!validateSchedule()) {
      return;
    }

    // Prepare schedule config for submission
    const scheduleConfig: ScheduleConfig = {
      numberOfLessons: slots.filter((s) => !s.isBreak).length,
      numberOfBreaks: slots.filter((s) => s.isBreak).length,
      startTime: schoolDayStart,
      endTime: schoolDayEnd,
      slots: slots,
    };

    onSubmit(scheduleConfig);
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-6">
      <div>
        <div className="my-6">
          <div className="flex justify-between items-center mb-2">
            <h3 className="text-sm text-center font-medium text-gray-900">Add the lesson and break structure for your school</h3>
            <div className="space-x-2">
              <button
                type="button"
                onClick={() => addSlot("lesson")}
                className="inline-flex items-center px-3 py-1 border border-transparent text-sm font-medium rounded-md text-white bg-sage hover:bg-[#7A979B]">
                Add Lesson
              </button>
              <button
                type="button"
                onClick={() => addSlot("break")}
                className="inline-flex items-center px-3 py-1 border border-gray-300 text-sm font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50">
                Add Break
              </button>
            </div>
          </div>

          <div className="bg-gray-50 rounded-md p-4">
            {slots.length === 0 ? (
              <div className="text-center py-4 text-gray-500">No slots added yet. Use the buttons above to add lessons and breaks.</div>
            ) : (
              <div className="space-y-3">
                {slots.map((slot, index) => (
                  <div
                    key={slot.id}
                    className={`rounded-md p-3 border ${slot.isBreak ? "bg-[#F7F7F7] border-[#D1D1D1]" : "bg-[#EEF2F3] border-sage"}`}>
                    <div className="flex flex-wrap items-center gap-2 mb-2">
                      {slot.isBreak ? (
                        <input
                          type="text"
                          value={slot.name}
                          onChange={(e) => updateSlot(slot.id, "name", e.target.value)}
                          className="flex-grow rounded-md border-gray-300 shadow-sm focus:border-sage focus:ring-sage sm:text-sm p-1 border min-w-0"
                          placeholder={"Break name"}
                        />
                      ) : (
                        <p className="flex-grow rounded-md  sm:text-sm p-1 min-w-0">{slot.name}</p>
                      )}

                      <div className="flex gap-1">
                        <button
                          type="button"
                          onClick={() => moveSlotUp(slot.id)}
                          disabled={index === 0}
                          className="p-1 text-gray-500 hover:text-gray-700 disabled:opacity-30"
                          title="Move up">
                          ↑
                        </button>
                        <button
                          type="button"
                          onClick={() => moveSlotDown(slot.id)}
                          disabled={index === slots.length - 1}
                          className="p-1 text-gray-500 hover:text-gray-700 disabled:opacity-30"
                          title="Move down">
                          ↓
                        </button>
                        <button type="button" onClick={() => removeSlot(slot.id)} className="text-xl text-red-500 hover:text-red-700" title="Remove">
                          ×
                        </button>
                      </div>
                    </div>

                    <div className="flex flex-wrap items-center gap-2">
                      <div className="flex items-center">
                        <span className="text-sm text-gray-500 mr-1">From</span>
                        <input
                          type="time"
                          value={slot.startTime}
                          onChange={(e) => updateSlot(slot.id, "startTime", e.target.value)}
                          className="rounded-md border-gray-300 shadow-sm focus:border-sage focus:ring-sage sm:text-sm p-1 border"
                          disabled={index === 0} // First slot must start at school start time
                        />
                      </div>

                      <div className="flex items-center">
                        <span className="text-sm text-gray-500 mr-1">To</span>
                        <input
                          type="time"
                          value={slot.endTime}
                          onChange={(e) => updateSlot(slot.id, "endTime", e.target.value)}
                          className="rounded-md border-gray-300 shadow-sm focus:border-sage focus:ring-sage sm:text-sm p-1 border"
                          disabled={index === slots.length - 1} // Last slot must end at school end time
                        />
                      </div>

                      <span className="text-xs text-gray-500">{calculateDuration(slot.startTime, slot.endTime)}</span>
                    </div>
                  </div>
                ))}
              </div>
            )}
          </div>
        </div>
      </div>

      {validationError && <div className="p-4 rounded-md text-sm bg-red-50 text-red-700">{validationError}</div>}

      <div className="flex justify-between">
        <button
          type="button"
          onClick={onBack}
          className="inline-flex justify-center rounded-md border border-gray-300 bg-white py-2 px-4 text-sm font-medium text-gray-700 shadow-sm hover:bg-gray-50">
          Back
        </button>
        <button
          type="submit"
          disabled={loading || slots.length === 0}
          className="inline-flex justify-center rounded-md border border-transparent bg-sage py-2 px-4 text-sm font-medium text-white shadow-sm hover:bg-[#7A979B] disabled:opacity-50">
          {loading ? <LoadingSpinner /> : "Continue"}
        </button>
      </div>
    </form>
  );
}

// Helper function to calculate and format the duration between two times
function calculateDuration(startTime: string, endTime: string): string {
  const [startHour, startMinute] = startTime.split(":").map(Number);
  const [endHour, endMinute] = endTime.split(":").map(Number);

  const startMinutes = startHour * 60 + startMinute;
  const endMinutes = endHour * 60 + endMinute;

  const durationMinutes = endMinutes - startMinutes;

  if (durationMinutes < 0) {
    return "Invalid duration";
  }

  const hours = Math.floor(durationMinutes / 60);
  const minutes = durationMinutes % 60;

  if (hours > 0) {
    return `${hours}h ${minutes}m`;
  } else {
    return `${minutes}m`;
  }
}
