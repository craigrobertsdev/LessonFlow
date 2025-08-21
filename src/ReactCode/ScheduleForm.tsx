import React, { useState, useRef, useEffect } from "react";
import { weekDays } from "lib/constants";
import type { TemplatePeriod, WeekPlannerTemplate } from "lib/types";
import type { ScheduleSlot } from "lib/accountSetup";
import { getSubjectColor } from "lib/utils";

// Interfaces for the day-based grid structure
interface TimeSlot {
  periodType: "lesson" | "break";
  subject?: string;
  breakDuty?: string;
  duration: number;
  startTime: string;
  endTime: string;
  isBreak: boolean;
}

interface DayColumn {
  dayOfWeek: string;
  isWorkingDay: boolean;
  timeSlots: (TimeSlot | null)[];
}

interface ScheduleFormProps {
  selectedSubjects: string[];
  workingDays: string[];
  scheduleSlots: ScheduleSlot[];
  onBack: () => void;
  onSubmit: (weekPlannerTemplate: WeekPlannerTemplate) => void;
  loading: boolean;
  savedState?: DayColumn[];
  onStateChange?: (dayGrid: DayColumn[]) => void;
}

export function ScheduleForm({
  selectedSubjects,
  workingDays,
  onBack,
  onSubmit,
  loading,
  scheduleSlots,
  savedState,
  onStateChange,
}: ScheduleFormProps) {
  const dialogRef = useRef<HTMLDialogElement>(null);

  const createBreakSlot = (slot: ScheduleSlot): TimeSlot => ({
    periodType: "break",
    duration: 1,
    startTime: slot.startTime,
    endTime: slot.endTime,
    isBreak: true,
  });

  const createEmptySlot = (slot: ScheduleSlot): TimeSlot | null => {
    return slot.isBreak ? createBreakSlot(slot) : null; // null represents an empty lesson slot that can be filled
  };

  const initialDayGrid: DayColumn[] = savedState
    ? savedState
    : weekDays.map((day) => ({
        dayOfWeek: day,
        isWorkingDay: workingDays.includes(day),
        timeSlots: scheduleSlots.map((slot) => createEmptySlot(slot)),
      }));

  const [dayGrid, setDayGrid] = useState<DayColumn[]>(initialDayGrid);
  const [selectedCell, setSelectedCell] = useState<{ dayIndex: number; slotIndex: number } | null>(null);
  const [selectedDuration, setSelectedDuration] = useState(1);
  const [hasGridChanged, setHasGridChanged] = useState(false);

  const updateDayGrid = (newGrid: DayColumn[]) => {
    setDayGrid(newGrid);
    setHasGridChanged(true);
  };

  useEffect(() => {
    if (hasGridChanged && onStateChange) {
      onStateChange(dayGrid);
      setHasGridChanged(false);
    }
  }, [hasGridChanged, dayGrid, onStateChange]);
  useEffect(() => {
    if (selectedCell) {
      const maxPeriods = calculateMaxAvailablePeriods(selectedCell.dayIndex, selectedCell.slotIndex);

      // If there's an existing lesson, set duration to current block size
      const existingSlot = dayGrid[selectedCell.dayIndex].timeSlots[selectedCell.slotIndex];
      if (existingSlot?.subject) {
        const blockInfo = getLessonBlockInfo(selectedCell.dayIndex, selectedCell.slotIndex);
        if (blockInfo) {
          setSelectedDuration(blockInfo.consecutiveCount);
          return;
        }
      }

      // Otherwise set to 1 or max available if current duration is too high
      if (selectedDuration > maxPeriods) {
        setSelectedDuration(Math.max(1, maxPeriods));
      }
    }
  }, [selectedCell]);

  useEffect(() => {
    if (selectedCell && dialogRef.current && !dialogRef.current.open) {
      dialogRef.current.showModal();
    } else if (!selectedCell && dialogRef.current && dialogRef.current.open) {
      dialogRef.current.close();
    }
  }, [selectedCell]);

  const handleCloseDialog = () => {
    setSelectedCell(null);
  };
  const handleCellClick = (dayIndex: number, slotIndex: number) => {
    const day = dayGrid[dayIndex];
    if (!day.isWorkingDay) return;

    const slot = day.timeSlots[slotIndex];
    if (slot?.periodType === "break") return;

    // If clicking on any part of a lesson (even if visually segmented by breaks),
    // find the start of the *logical* lesson block for selection.
    if (slot?.periodType === "lesson" && slot?.subject) {
      const logicalBlockInfo = getLessonBlockInfo(dayIndex, slotIndex);
      if (logicalBlockInfo) {
        setSelectedCell({ dayIndex, slotIndex: logicalBlockInfo.startIndex });
      } else {
        // Should not happen if slot.subject is defined, but as a fallback:
        setSelectedCell({ dayIndex, slotIndex });
      }
    } else {
      // Clicked on an empty, non-break cell
      setSelectedCell({ dayIndex, slotIndex });
    }
  }; // Calculate the maximum number of consecutive periods available from a starting position
  const calculateMaxAvailablePeriods = (dayIndex: number, startSlotIndex: number): number => {
    const day = dayGrid[dayIndex];
    if (!day.isWorkingDay || startSlotIndex >= day.timeSlots.length) return 0;

    // If there's already a lesson at this position, get its current block size
    const existingSlot = day.timeSlots[startSlotIndex];
    if (existingSlot?.subject) {
      const blockInfo = getLessonBlockInfo(dayIndex, startSlotIndex);
      if (blockInfo) {
        // Start counting from the existing block size
        let availablePeriods = blockInfo.consecutiveCount;
        let currentSlotIndex = blockInfo.endIndex + 1;

        // Skip any breaks after the current block
        while (currentSlotIndex < day.timeSlots.length && scheduleSlots[currentSlotIndex].isBreak) {
          currentSlotIndex++;
        }

        // Count additional available periods
        while (currentSlotIndex < day.timeSlots.length) {
          const slot = day.timeSlots[currentSlotIndex];

          // Skip breaks when calculating available periods
          if (scheduleSlots[currentSlotIndex].isBreak) {
            currentSlotIndex++;
            continue;
          }

          // If it's already a different lesson, we can't use this slot
          if (slot?.periodType === "lesson" && slot?.subject) {
            break;
          }

          availablePeriods++;
          currentSlotIndex++;

          // Skip any breaks in between lessons
          while (currentSlotIndex < day.timeSlots.length && scheduleSlots[currentSlotIndex].isBreak) {
            currentSlotIndex++;
          }
        }

        return availablePeriods;
      }
    }

    // No existing lesson, count normally
    let availablePeriods = 0;
    let currentSlotIndex = startSlotIndex;

    while (currentSlotIndex < day.timeSlots.length) {
      const slot = day.timeSlots[currentSlotIndex];

      // Skip breaks when calculating available periods
      if (scheduleSlots[currentSlotIndex].isBreak) {
        currentSlotIndex++;
        continue;
      }

      // If it's already a different lesson, we can't use this slot
      if (slot?.periodType === "lesson" && slot?.subject) {
        break;
      }

      availablePeriods++;
      currentSlotIndex++;

      // Skip any breaks in between lessons
      while (currentSlotIndex < day.timeSlots.length && scheduleSlots[currentSlotIndex].isBreak) {
        currentSlotIndex++;
      }
    }
    return availablePeriods;
  }; // Determine if a cell should be rendered (used for handling multi-period spans)
  const shouldRenderCell = (dayIndex: number, slotIndex: number): boolean => {
    const day = dayGrid[dayIndex];
    const slot = day.timeSlots[slotIndex];

    // Breaks should ALWAYS be rendered
    if (scheduleSlots[slotIndex].isBreak) return true;

    // Empty lesson slots should be rendered
    if (!slot?.subject) return true;

    // For lesson slots, check if this cell is the start of a visual segment
    const visualInfo = getVisualLessonSegmentInfo(dayIndex, slotIndex);
    if (visualInfo && slotIndex === visualInfo.startIndex) {
      return true; // It's the first cell of a visual segment
    }

    // If it's part of a visual segment but not the start, or not a lesson, don't render (it's covered)
    if (visualInfo && slotIndex > visualInfo.startIndex) {
      return false;
    }

    // Fallback for any other case (e.g. a lesson slot that somehow didn't form a visual segment - should not happen)
    return true;
  }; // Get information about a lesson block (consecutive lessons of same subject)
  const getLessonBlockInfo = (dayIndex: number, clickedSlotIndex: number) => {
    const day = dayGrid[dayIndex];
    const clickedSlot = day.timeSlots[clickedSlotIndex];

    if (!clickedSlot?.subject) return null;

    const subject = clickedSlot.subject;

    // Find the true start of the block by looking backward
    let startIndex = clickedSlotIndex;
    for (let i = clickedSlotIndex - 1; i >= 0; i--) {
      // Skip breaks, but continue looking
      if (scheduleSlots[i].isBreak) {
        continue;
      }

      const currentSlot = day.timeSlots[i];
      if (currentSlot?.subject === subject) {
        startIndex = i;
      } else {
        // Different subject or empty slot - stop looking
        break;
      }
    }

    // Find the true end of the block by looking forward
    let endIndex = clickedSlotIndex;
    for (let i = clickedSlotIndex + 1; i < day.timeSlots.length; i++) {
      // Skip breaks, but continue looking
      if (scheduleSlots[i].isBreak) {
        continue;
      }

      const currentSlot = day.timeSlots[i];
      if (currentSlot?.subject === subject) {
        endIndex = i;
      } else {
        // Different subject or empty slot - stop looking
        break;
      }
    }

    // Count the total consecutive periods in the block (excluding breaks)
    let consecutiveCount = 0;
    for (let i = startIndex; i <= endIndex; i++) {
      if (!scheduleSlots[i].isBreak && day.timeSlots[i]?.subject === subject) {
        consecutiveCount++;
      }
    }

    return {
      startIndex,
      endIndex,
      consecutiveCount,
      subject,
    };
  };
  // Get information about a visual lesson segment (consecutive lessons of same subject, NOT spanning breaks)
  const getVisualLessonSegmentInfo = (dayIndex: number, slotIndex: number) => {
    const day = dayGrid[dayIndex];
    const initialSlot = day.timeSlots[slotIndex];

    if (!initialSlot?.subject || scheduleSlots[slotIndex].isBreak) {
      return null; // Not a lesson or it's a break slot itself
    }

    const subject = initialSlot.subject;
    let segmentStartIndex = slotIndex;
    let segmentEndIndex = slotIndex;

    // Look backward for the start of the visual segment
    for (let i = slotIndex - 1; i >= 0; i--) {
      if (scheduleSlots[i].isBreak) {
        break; // Break ends the visual segment
      }
      const currentSlot = day.timeSlots[i];
      if (currentSlot?.subject === subject) {
        segmentStartIndex = i;
      } else {
        break; // Different subject or empty slot
      }
    }

    // Look forward for the end of the visual segment
    for (let i = slotIndex + 1; i < day.timeSlots.length; i++) {
      if (scheduleSlots[i].isBreak) {
        break; // Break ends the visual segment
      }
      const currentSlot = day.timeSlots[i];
      if (currentSlot?.subject === subject) {
        segmentEndIndex = i;
      } else {
        break; // Different subject or empty slot
      }
    }

    return {
      startIndex: segmentStartIndex,
      endIndex: segmentEndIndex,
      subject: subject,
      count: segmentEndIndex - segmentStartIndex + 1,
    };
  };
  // Handle subject selection in the dialog
  const handleSubjectSelect = (subject: string) => {
    if (!selectedCell) return;

    const { dayIndex, slotIndex } = selectedCell;

    // Create individual lesson slots for the selected duration, skipping breaks
    const newGrid = [...dayGrid];
    const day = { ...newGrid[dayIndex] };
    newGrid[dayIndex] = day;

    const newTimeSlots = [...day.timeSlots];
    day.timeSlots = newTimeSlots;

    // Clear any existing lessons in this block first
    const existingSlot = newTimeSlots[slotIndex];
    if (existingSlot?.subject) {
      const blockInfo = getLessonBlockInfo(dayIndex, slotIndex);
      if (blockInfo) {
        for (let i = blockInfo.startIndex; i <= blockInfo.endIndex; i++) {
          if (!scheduleSlots[i].isBreak) {
            newTimeSlots[i] = null;
          }
        }
      }
    }

    // Place individual lesson slots starting from the selected position
    let currentIndex = slotIndex;
    let lessonsPlaced = 0;

    while (lessonsPlaced < selectedDuration && currentIndex < scheduleSlots.length) {
      // Skip breaks - don't place lessons in break slots
      if (scheduleSlots[currentIndex].isBreak) {
        currentIndex++;
        continue;
      }

      // Clear the current slot and place the lesson (each as individual cell)
      newTimeSlots[currentIndex] = {
        periodType: "lesson",
        subject,
        duration: 1, // Always 1 for individual rendering, visual spanning handled by table
        startTime: scheduleSlots[currentIndex].startTime,
        endTime: scheduleSlots[currentIndex].endTime,
        isBreak: scheduleSlots[currentIndex].isBreak,
      };

      lessonsPlaced++;
      currentIndex++;
    }

    localStorage.setItem("setup_scheduleGrid", JSON.stringify(newGrid));

    updateDayGrid(newGrid);
    setSelectedCell(null);
  };
  const handleClearCell = () => {
    if (!selectedCell) return;
    const { dayIndex, slotIndex } = selectedCell;
    const slot = dayGrid[dayIndex].timeSlots[slotIndex];

    if (slot?.periodType === "lesson") {
      const newGrid = [...dayGrid];
      const day = { ...newGrid[dayIndex] };
      newGrid[dayIndex] = day;

      const newTimeSlots = [...day.timeSlots];
      day.timeSlots = newTimeSlots;

      // Find and clear the entire lesson block
      const blockInfo = getLessonBlockInfo(dayIndex, slotIndex);
      if (blockInfo) {
        for (let i = blockInfo.startIndex; i <= blockInfo.endIndex; i++) {
          if (!scheduleSlots[i].isBreak) {
            newTimeSlots[i] = null;
          }
        }
      } else {
        // Fallback: clear just this individual lesson slot
        newTimeSlots[slotIndex] = null;
      }

      updateDayGrid(newGrid);
      setSelectedCell(null);
    }
  };

  // Handle break duty name changes
  const handleBreakDutyChange = (dayIndex: number, slotIndex: number, newDuty: string) => {
    const newGrid = [...dayGrid];
    const day = { ...newGrid[dayIndex] };
    newGrid[dayIndex] = day;

    const newTimeSlots = [...day.timeSlots];
    day.timeSlots = newTimeSlots;

    const slot = newTimeSlots[slotIndex];
    if (slot?.periodType === "break") {
      newTimeSlots[slotIndex] = {
        ...slot,
        breakDuty: newDuty,
      };
      updateDayGrid(newGrid);
    }
  };

  // Calculate lesson numbers in sequence (1, 2, 3...)
  const getLessonNumbers = (): number[] => {
    const result: number[] = [];
    let currentLessonNumber = 1;

    scheduleSlots.forEach((slot) => {
      if (slot.isBreak) {
        result.push(0); // 0 indicates a break
      } else {
        result.push(currentLessonNumber);
        currentLessonNumber++;
      }
    });

    return result;
  };

  const lessonNumbers = getLessonNumbers();
  // Convert day-based grid to WeekPlannerTemplate format
  const convertGridToWeekPlannerTemplate = (): WeekPlannerTemplate => {
    // Create periods array from scheduleSlots
    const periods: TemplatePeriod[] = scheduleSlots.map((slot) => ({
      startTime: slot.startTime,
      endTime: slot.endTime,
      isBreak: slot.isBreak,
      name: slot.name,
    }));

    // Create day templates from the dayGrid
    const dayTemplates = dayGrid.map((day, currentDayIndex) => ({
      dayOfWeek: day.dayOfWeek,
      type: day.isWorkingDay ? ("workingDay" as const) : ("nwd" as const),
      templates: day.isWorkingDay
        ? day.timeSlots.reduce<
            Array<{
              periodType: "lesson" | "break";
              numberOfPeriods: number;
              startPeriod: number;
              subjectName?: string;
              breakDuty?: string;
            }>
          >((acc, slot, slotIndex) => {
            if (!day.isWorkingDay) return acc; // Should be caught by outer check, but good for safety

            const currentSlotInfo = day.timeSlots[slotIndex];

            if (scheduleSlots[slotIndex].isBreak) {
              // Add break
              if (currentSlotInfo) {
                // Ensure break slot exists in grid
                acc.push({
                  periodType: "break",
                  numberOfPeriods: 1,
                  startPeriod: slotIndex + 1, // API is 1-based
                  breakDuty: currentSlotInfo.breakDuty,
                });
              }
            } else if (currentSlotInfo?.periodType === "lesson" && currentSlotInfo.subject) {
              // It's a lesson slot, get its logical block information
              const logicalBlockInfo = getLessonBlockInfo(currentDayIndex, slotIndex);
              if (logicalBlockInfo && logicalBlockInfo.startIndex === slotIndex) {
                // Only add the lesson if we are at the START of its LOGICAL block
                acc.push({
                  periodType: "lesson",
                  numberOfPeriods: logicalBlockInfo.consecutiveCount, // Total lesson periods in the logical block
                  startPeriod: logicalBlockInfo.startIndex + 1, // API is 1-based
                  subjectName: logicalBlockInfo.subject,
                });
              }
              // If it's a lesson slot but not the start of its logical block, we ignore it
              // as it's already accounted for by its starting slot.
            }
            // Empty non-break slots are just ignored for the template

            return acc;
          }, [])
        : [], // Empty array for non-working days
    }));

    return {
      periods,
      dayTemplates,
    };
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    const weekPlannerTemplate = convertGridToWeekPlannerTemplate();
    onSubmit(weekPlannerTemplate);
  };

  return (
    <div className="max-w-4xl mx-auto px-4 text-center">
      <form onSubmit={handleSubmit} className="space-y-8">
        <div className="w-full">
          <table className="w-full table-fixed divide-y divide-gray-200">
            <colgroup>
              <col className="w-32" />
              <col className="w-[calc((100%-8rem)/5)]" span={5} />
            </colgroup>
            <thead>
              <tr className="divide-x divide-gray-200">
                <th className="px-6 py-3 bg-gray-50 text-xs font-medium text-gray-500 uppercase tracking-wider">Time</th>
                {dayGrid.map((day, dayIndex) => (
                  <th
                    key={day.dayOfWeek}
                    className={`px-6 py-3 text-xs font-medium uppercase tracking-wider
                      ${day.isWorkingDay ? "bg-gray-50 text-gray-500" : "bg-gray-100 text-gray-400"}`}>
                    {day.dayOfWeek}
                  </th>
                ))}
              </tr>
            </thead>
            <tbody className="bg-white divide-y divide-gray-200">
              {scheduleSlots.map((scheduleSlot, slotIndex) => (
                <tr key={slotIndex} className="divide-x divide-gray-200">
                  <td className={`${scheduleSlot.isBreak ? "px-2 py-1" : "px-6 py-4"} whitespace-nowrap text-sm text-gray-500`}>
                    <div className="font-medium mb-1">
                      {scheduleSlot.isBreak ? scheduleSlot.name || "Break" : `Lesson ${lessonNumbers[slotIndex]}`}
                    </div>
                    <div className="text-gray-400">
                      {scheduleSlot.startTime} - {scheduleSlot.endTime}
                    </div>
                  </td>{" "}
                  {dayGrid.map((day, dayIndex) =>
                    shouldRenderCell(dayIndex, slotIndex) ? (
                      <td
                        key={dayIndex}
                        onClick={() => (day.isWorkingDay ? handleCellClick(dayIndex, slotIndex) : null)}
                        className={`px-2 py-1 text-sm overflow-hidden text-ellipsis text-center 
                          ${
                            !day.isWorkingDay
                              ? "bg-gray-100"
                              : day.timeSlots[slotIndex]?.periodType === "break"
                              ? "bg-gray-200 text-gray-500"
                              : "cursor-pointer"
                          }
                          ${selectedCell?.dayIndex === dayIndex && selectedCell?.slotIndex === slotIndex ? "ring-2 ring-sage" : ""}
                          ${day.isWorkingDay && !day.timeSlots[slotIndex]?.periodType ? "hover:bg-gray-50" : ""}
                          ${
                            day.timeSlots[slotIndex]?.periodType === "lesson" && day.timeSlots[slotIndex]?.subject
                              ? getSubjectColor(day.timeSlots[slotIndex]?.subject || "")
                              : ""
                          }`}
                        {...(() => {
                          // Calculate rowSpan for visual lesson segments
                          const slot = day.timeSlots[slotIndex];
                          if (slot?.periodType === "lesson" && slot?.subject && !scheduleSlots[slotIndex].isBreak) {
                            const visualInfo = getVisualLessonSegmentInfo(dayIndex, slotIndex);
                            if (visualInfo && visualInfo.count > 0 && visualInfo.startIndex === slotIndex) {
                              return { rowSpan: visualInfo.count };
                            }
                          }
                          return {}; // Default, no rowspan
                        })()}>
                        {day.isWorkingDay && day.timeSlots[slotIndex]?.periodType === "break" ? (
                          <input
                            type="text"
                            value={day.timeSlots[slotIndex]?.breakDuty || ""}
                            onChange={(e) => handleBreakDutyChange(dayIndex, slotIndex, e.target.value)}
                            className="w-full bg-gray-100 border border-gray-300 rounded px-2 py-1 text-gray-700 hover:bg-gray-200 focus:outline-none focus:ring-1 focus:ring-sage focus:border-sage"
                          />
                        ) : day.isWorkingDay ? (
                          day.timeSlots[slotIndex]?.subject || ""
                        ) : (
                          ""
                        )}
                      </td>
                    ) : null
                  )}
                </tr>
              ))}
            </tbody>
          </table>
        </div>

        {selectedCell && (
          <dialog
            ref={dialogRef}
            className="p-0 text-black shadow-xl backdrop:bg-gray-100 backdrop:opacity-30 mx-auto my-auto"
            onCancel={handleCloseDialog}>
            <div className="bg-white p-6 max-w-md w-full">
              <h3 className="text-lg font-medium text-gray-900 mb-4">Select Subject</h3>

              <div className="space-y-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700">Duration (Periods)</label>
                  <select
                    value={selectedDuration}
                    onChange={(e) => setSelectedDuration(Number(e.target.value))}
                    className="mt-1 block w-full pl-3 pr-10 py-2.5 text-base border-2 border-sage bg-white focus:outline-none focus:ring-2 focus:ring-sage focus:border-sage sm:text-sm rounded-md shadow-sm">
                    {selectedCell
                      ? Array.from({ length: calculateMaxAvailablePeriods(selectedCell.dayIndex, selectedCell.slotIndex) }, (_, i) => i + 1).map(
                          (num) => (
                            <option key={num} value={num}>
                              {num}
                            </option>
                          )
                        )
                      : null}
                  </select>
                </div>

                <div className="grid grid-cols-2 gap-4">
                  {selectedSubjects.map((subject) => (
                    <button
                      key={subject}
                      type="button"
                      onClick={() => handleSubjectSelect(subject)}
                      className={`inline-flex justify-center items-center px-3 py-2 text-sm font-medium ${getSubjectColor(
                        subject
                      )} rounded-md hover:opacity-90`}>
                      {subject}
                    </button>
                  ))}
                  <button
                    key="nit"
                    type="button"
                    onClick={() => handleSubjectSelect("NIT")}
                    className={`inline-flex justify-center items-center px-3 py-2 text-sm font-medium rounded-md ${getSubjectColor(
                      "NIT"
                    )} hover:opacity-90`}>
                    NIT
                  </button>
                </div>

                {selectedCell && dayGrid[selectedCell.dayIndex].timeSlots[selectedCell.slotIndex]?.periodType === "lesson" && (
                  <button
                    type="button"
                    onClick={handleClearCell}
                    className="w-full inline-flex justify-center items-center px-3 py-2 border border-red-300 shadow-sm text-sm font-medium rounded-md text-red-700 bg-white hover:bg-red-50">
                    Clear Cell
                  </button>
                )}

                <div className="mt-6 flex justify-end">
                  <button
                    type="button"
                    onClick={handleCloseDialog}
                    className="inline-flex items-center px-4 py-2 border border-gray-300 shadow-sm text-sm font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50">
                    Cancel
                  </button>
                </div>
              </div>
            </div>
          </dialog>
        )}

        <div className="flex justify-between mt-6">
          <button
            type="button"
            onClick={onBack}
            className="inline-flex justify-center rounded-md border border-gray-300 bg-white py-2 px-4 text-sm font-medium text-gray-700 shadow-sm hover:bg-gray-50">
            Back
          </button>
          <button
            type="submit"
            disabled={loading}
            className="inline-flex justify-center rounded-md border border-transparent bg-sage py-2 px-4 text-sm font-medium text-white shadow-sm hover:opacity-90 disabled:opacity-50">
            Complete Setup
          </button>
        </div>
      </form>
    </div>
  );
}
