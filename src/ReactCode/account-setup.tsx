import React, { useState, useEffect, useTransition } from "react";
import { Navigate, redirect, useNavigate, useSearchParams } from "react-router";
import { ApiClient } from "lib/apiClient";
import type { AccountSetupRequest, WeekPlannerTemplate } from "lib/types";
import { BasicInfoForm } from "../components/account-setup/BasicInfoForm";
import { SubjectsForm } from "../components/account-setup/SubjectsForm";
import { ScheduleForm } from "../components/account-setup/ScheduleForm";
import { TimingForm } from "../components/account-setup/TimingForm";
import type { ScheduleConfig } from "../components/account-setup/TimingForm";
import { ProgressSteps } from "../components/account-setup/ProgressSteps";
import { LoadingSpinner } from "../components/LoadingSpinner";
import { useAuth } from "../contexts/AuthContext";
import type { Route } from "./+types/account-setup";

export async function clientLoader() {
  const apiClient = new ApiClient();

  const response = await apiClient.checkUser();
  if (!response.isAuthenticated) {
    localStorage.removeItem("accountData");
    localStorage.removeItem("token");
    return redirect("/auth/login");
  }

  if (response.accountData!.user.hasCompletedAccountSetup) {
    return redirect(`/app/week-planner/${new Date().getFullYear()}/${response.accountData!.currentTerm}/${response.accountData!.currentWeek}`);
  }

  const subjectNames = await apiClient.getCurriculumSubjectNames();
  return subjectNames;
}

interface UserUpdate {
  hasCompletedAccountSetup: boolean;
  schoolName: string;
}
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

const apiClient = new ApiClient();

type Step = "info" | "subjects" | "timing" | "schedule";
const steps: Step[] = ["info", "subjects", "timing", "schedule"];

const weekDays = ["Monday", "Tuesday", "Wednesday", "Thursday", "Friday"];

export default function AccountSetup({ loaderData }: Route.ComponentProps) {
  const navigate = useNavigate();
  const { logout, user, updateUser, currentTermData, setCurrentTermData } = useAuth();
  const [searchParams, setSearchParams] = useSearchParams();

  const [currentStep, setCurrentStep] = useState<Step>(() => {
    const stepParam = searchParams.get("step") as Step;
    return stepParam && ["info", "subjects", "timing", "schedule"].includes(stepParam) ? stepParam : "info";
  });

  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const [schoolName, setSchoolName] = useState<string>(() => {
    return localStorage.getItem("setup_schoolName") || "";
  });

  const [calendarYear, setCalendarYear] = useState<number>(() => {
    const savedYear = localStorage.getItem("setup_calendarYear");
    return savedYear ? parseInt(savedYear) : new Date().getFullYear();
  });

  const [yearLevelsTaught, setYearLevelsTaught] = useState<string[]>(() => {
    const savedLevels = localStorage.getItem("setup_yearLevelsTaught");
    return savedLevels ? JSON.parse(savedLevels) : [];
  });

  const [subjectsTaught, setSubjectsTaught] = useState<string[]>(() => {
    const savedSubjects = localStorage.getItem("setup_subjectsTaught");
    return savedSubjects ? JSON.parse(savedSubjects) : [];
  });

  const [workingDays, setWorkingDays] = useState<string[]>(() => {
    const savedDays = localStorage.getItem("setup_workingDays");
    return savedDays ? JSON.parse(savedDays) : weekDays;
  });

  const [scheduleGrid, setScheduleGrid] = useState<DayColumn[] | undefined>(() => {
    const savedSchedule = localStorage.getItem("setup_scheduleGrid");
    return savedSchedule ? JSON.parse(savedSchedule) : undefined;
  });

  const [scheduleConfig, setScheduleConfig] = useState<ScheduleConfig>(() => {
    const savedConfig = localStorage.getItem("setup_scheduleConfig");
    return savedConfig
      ? JSON.parse(savedConfig)
      : {
          numberOfLessons: 6,
          numberOfBreaks: 2,
          startTime: "09:00",
          endTime: "15:00",
          slots: [
            { id: "1", isBreak: false, name: "Lesson 1", startTime: "09:10", endTime: "10:00" },
            { id: "2", isBreak: false, name: "Lesson 2", startTime: "10:00", endTime: "10:50" },
            { id: "3", isBreak: true, name: "Recess", startTime: "10:50", endTime: "11:20" },
            { id: "4", isBreak: false, name: "Lesson 3", startTime: "11:20", endTime: "12:10" },
            { id: "5", isBreak: false, name: "Lesson 4", startTime: "12:10", endTime: "13:00" },
            { id: "6", isBreak: true, name: "Lunch", startTime: "13:00", endTime: "13:30" },
            { id: "7", isBreak: false, name: "Lesson 5", startTime: "13:30", endTime: "14:20" },
            { id: "8", isBreak: false, name: "Lesson 6", startTime: "14:20", endTime: "15:10" },
          ],
        };
  });

  const [completedSteps, setCompletedSteps] = useState<Set<Step>>(() => {
    const savedSteps = localStorage.getItem("setup_completedSteps");
    if (savedSteps) {
      try {
        return new Set(JSON.parse(savedSteps) as Step[]);
      } catch (e) {
        console.error("Error parsing saved steps:", e);
      }
    }
    return new Set(["info"]);
  });

  const [isPending, startTransition] = useTransition();

  const updateStep = (step: Step) => {
    startTransition(() => {
      setCompletedSteps((prev) => {
        const newSet = new Set(prev);
        newSet.add(step);
        return newSet;
      });

      setCurrentStep(step);
      setSearchParams({ step });
    });
  };

  const handleBackNavigation = (previousStep: Step) => {
    startTransition(() => {
      setCurrentStep(previousStep);
      setSearchParams({ step: previousStep });
    });
  };

  const isNextStep = (step: Step): boolean => {
    const currentIndex = steps.indexOf(currentStep);
    const targetIndex = steps.indexOf(step);
    return targetIndex === currentIndex + 1;
  };

  const getLastCompletedStep = (): Step => {
    let lastStep: Step = "info";

    for (const step of steps) {
      if (completedSteps.has(step)) {
        lastStep = step;
      } else {
        break;
      }
    }

    return lastStep;
  };

  useEffect(() => {
    const stepParam = searchParams.get("step") as Step;

    if (stepParam && steps.includes(stepParam)) {
      if (completedSteps.has(stepParam) || isNextStep(stepParam) || stepParam === "info") {
        setCurrentStep(stepParam);

        if (window.location.search !== `?step=${stepParam}`) {
          setSearchParams({ step: stepParam });
        }
      } else {
        const lastCompletedStep = getLastCompletedStep();
        setCurrentStep(lastCompletedStep);
        setSearchParams({ step: lastCompletedStep });
      }
    } else if (!stepParam) {
      const lastCompletedStep = getLastCompletedStep();
      setCurrentStep(lastCompletedStep);
      setSearchParams({ step: lastCompletedStep });
    }
  }, [searchParams, completedSteps]);

  useEffect(() => {
    localStorage.setItem("setup_completedSteps", JSON.stringify(Array.from(completedSteps)));
  }, [completedSteps]);

  if (user?.hasCompletedAccountSetup) {
    return <Navigate to={`/app/week-planner/${new Date().getFullYear()}/${currentTermData?.termNumber}/${currentTermData?.weekNumber}`} replace />;
  }

  const clearSetupData = () => {
    const keys = [
      "setup_firstName",
      "setup_schoolName",
      "setup_calendarYear",
      "setup_yearLevelsTaught",
      "setup_subjectsTaught",
      "setup_workingDays",
      "setup_scheduleGrid",
      "setup_scheduleConfig",
      "setup_completedSteps",
    ];

    keys.forEach((key) => localStorage.removeItem(key));
  };

  const convertYearLevelsTaughtToApiFormat = (levels: string[]): string[] => {
    return yearLevelsTaught.map((level) => {
      if (level === "Foundation") return level;
      return level.replace(" ", "");
    });
  };

  const handleInfoSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    if (!schoolName.trim()) {
      setError("Please enter your school name");
      return;
    }

    if (yearLevelsTaught.length === 0) {
      setError("Please select at least one year level");
      return;
    }

    if (workingDays.length === 0) {
      setError("Please select at least one working day");
      return;
    }

    localStorage.setItem("setup_schoolName", schoolName.trim());
    localStorage.setItem("setup_calendarYear", calendarYear.toString());
    localStorage.setItem("setup_yearLevelsTaught", JSON.stringify(yearLevelsTaught));
    localStorage.setItem("setup_workingDays", JSON.stringify(workingDays));

    updateStep("subjects");
  };

  const handleSubjectsSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    if (subjectsTaught.length === 0) {
      setError("Please select at least one subject");
      return;
    }

    localStorage.setItem("setup_subjectsTaught", JSON.stringify(subjectsTaught));

    updateStep("timing");
  };

  const handleTimingSubmit = (config: ScheduleConfig) => {
    localStorage.setItem("setup_scheduleConfig", JSON.stringify(config));

    setScheduleConfig(config);
    updateStep("schedule");
  };

  const handleScheduleSubmit = async (weekPlannerTemplate: WeekPlannerTemplate) => {
    try {
      setLoading(true);
      setError(null);

      const yearLevels = convertYearLevelsTaughtToApiFormat(yearLevelsTaught);

      const setupData: AccountSetupRequest = {
        schoolName: schoolName.trim(),
        subjectsTaught,
        weekPlannerTemplate: weekPlannerTemplate,
        yearLevelsTaught: yearLevels,
        calendarYear,
        workingDays,
      };

      const data = await apiClient.completeSetup(setupData);

      const userUpdate: UserUpdate = {
        schoolName: setupData.schoolName,
        hasCompletedAccountSetup: true,
      };
      updateUser(userUpdate);
      setCurrentTermData({ termNumber: data.currentTerm, weekNumber: data.currentWeek });

      clearSetupData();
      navigate(`/app/week-planner/${new Date().getFullYear()}/${currentTermData?.termNumber}/${currentTermData?.weekNumber}`);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to complete account setup");
      console.error("Error during setup:", err);
    } finally {
      setLoading(false);
    }
  };

  if (loading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <LoadingSpinner />
      </div>
    );
  }

  return (
    <>
      <title>LessonFlow - Account Setup</title>
      <div className="min-h-screen bg-gray-50">
        <div className="absolute top-5 right-5 z-10">
          <button
            onClick={() => {
              logout();
              clearSetupData();
              window.location.href = "/auth/login";
            }}
            className="text-gray-600 hover:text-gray-800 bg-white hover:bg-gray-50 hover:cursor-pointer px-3 py-2 rounded-md border border-gray-300 shadow-sm text-sm font-medium transition-colors">
            Logout
          </button>
        </div>

        <div className="max-w-7xl mx-auto py-12 px-4 sm:px-6 lg:px-8">
          <div className="mb-8 max-w-3xl mx-auto text-center">
            <h2 className="text-3xl font-extrabold text-gray-900 text-center">Welcome to LessonFlow</h2>
            <p className="mt-4 text-lg text-gray-500 text-center">Let's get your account set up so you can start planning your lessons.</p>

            {error && (
              <div className="mt-4 p-4 text-sm text-red-700 bg-red-100 rounded-lg" role="alert">
                {error}
              </div>
            )}

            <div className="mt-8">
              <ProgressSteps currentStep={currentStep} />
            </div>
          </div>

          <div className={`mx-auto ${currentStep === "schedule" ? "max-w-none" : "max-w-xl"}`}>
            {currentStep === "info" && (
              <BasicInfoForm
                schoolName={schoolName}
                setSchoolName={setSchoolName}
                calendarYear={calendarYear}
                setCalendarYear={setCalendarYear}
                yearLevelsTaught={yearLevelsTaught}
                setYearLevelsTaught={setYearLevelsTaught}
                workingDays={workingDays}
                setWorkingDays={setWorkingDays}
                onSubmit={handleInfoSubmit}
                loading={loading}
                error={error}
              />
            )}

            {currentStep === "subjects" && (
              <SubjectsForm
                subjects={loaderData.subjectNames}
                selectedSubjects={subjectsTaught}
                setSubjectsTaught={setSubjectsTaught}
                onBack={() => {
                  setError(null);
                  handleBackNavigation("info");
                }}
                onSubmit={handleSubjectsSubmit}
                loading={loading}
              />
            )}

            {currentStep === "timing" && (
              <TimingForm
                scheduleSlots={scheduleConfig.slots}
                onBack={() => {
                  setError(null);
                  handleBackNavigation("subjects");
                }}
                onSubmit={handleTimingSubmit}
                loading={loading}
              />
            )}

            {currentStep === "schedule" && (
              <ScheduleForm
                selectedSubjects={subjectsTaught}
                workingDays={workingDays}
                scheduleSlots={scheduleConfig.slots}
                savedState={scheduleGrid}
                onStateChange={(state) => setScheduleGrid(state)}
                onBack={() => {
                  setError(null);
                  handleBackNavigation("timing");
                }}
                onSubmit={handleScheduleSubmit}
                loading={loading}
              />
            )}
          </div>
        </div>
      </div>
    </>
  );
}
