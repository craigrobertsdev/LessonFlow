import { CheckIcon } from "@heroicons/react/24/solid";

type Step = "info" | "subjects" | "timing" | "schedule";
type Status = "complete" | "current" | "upcoming";

interface ProgressStepsProps {
  currentStep: Step;
}

const steps = [
  { id: "info", label: "School Info" },
  { id: "subjects", label: "Subjects" },
  { id: "timing", label: "Timing" },
  { id: "schedule", label: "Schedule" },
] as const;

export function ProgressSteps({ currentStep }: ProgressStepsProps) {
  const getStepStatus = (stepId: Step): Status => {
    const currentIndex = steps.findIndex((s) => s.id === currentStep);
    const stepIndex = steps.findIndex((s) => s.id === stepId);

    if (stepIndex < currentIndex) return "complete";
    if (stepIndex === currentIndex) return "current";
    return "upcoming";
  };

  return (
    <nav aria-label="Progress">
      <ol className="flex items-center">
        {steps.map((step) => (
          <li
            key={step.id}
            className={`md:flex-1 pb-2 border-b-2 ${getStepStatus(step.id as Step) === "complete" ? "border-sage" : "border-gray-200"} `}>
            <div className="group flex flex-col py-2 pl-4 md:pl-0 md:pt-4 md:pb-0">
              <span
                className={`
                  mt-0.5 flex items-center text-sm font-medium justify-center
                  ${getStepStatus(step.id as Step) === "complete" ? "text-sage" : ""}
                  ${getStepStatus(step.id as Step) === "current" ? "text-sage" : ""}
                  ${getStepStatus(step.id as Step) === "upcoming" ? "text-gray-500" : ""}
                `}>
                {step.label}
                <span
                  className={`ml-2 h-5 w-5 flex items-center justify-center rounded-full 
                      ${getStepStatus(step.id as Step) === "complete" ? "bg-sage" : ""}
                      ${getStepStatus(step.id as Step) === "current" ? "border-2 border-sage" : ""}
                      ${getStepStatus(step.id as Step) === "upcoming" ? "border-2 border-gray-300" : ""}`}>
                  {getStepStatus(step.id as Step) === "complete" ? (
                    <CheckIcon className="h-3 w-3 text-white" />
                  ) : (
                    <span className={`h-2.5 w-2.5 rounded-full ${getStepStatus(step.id as Step) === "current" ? "bg-sage" : ""}`} />
                  )}
                </span>
              </span>
            </div>
          </li>
        ))}
      </ol>
    </nav>
  );
}
