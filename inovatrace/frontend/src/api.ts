export type ProjectListItem = {
  id: string;
  title: string;
  group: string;
  team: string;
  durationWeeks: number;
  queueStatus: string;
  recommendation: string | null;
  analysisId: string | null;
  warningCount: number;
};

export type Criterion = {
  criterion: string;
  label: string;
  status: string;
  statusLabel: string;
  confidence: number;
  reasoning: string;
  claimCodes: string[];
  supportingEvidenceIds: string[];
  contraryEvidenceIds: string[];
  missingEvidenceIds: string[];
};

export type Claim = {
  code: string;
  activityId: string | null;
  text: string;
  source: string;
  confidence: number;
  evidenceIds: string[];
};

export type Analysis = {
  id: string;
  projectId: string;
  projectTitle: string;
  team: string;
  lifecycle: string;
  recommendation: string;
  summary: string;
  limit: string;
  confidence: number;
  requiresHumanReview: boolean;
  counts: { favorable: number; contrary: number; contradictory: number; absent: number };
  criteria: Criterion[];
  claims: Claim[];
  contradictions: { sourceA: string; sourceB: string; claimA: string | null; claimB: string | null; affectedCriterion: string; severity: string; explanation: string }[];
  missing: { affectedCriterion: string; description: string; neededEvidence: string }[];
  mathChecks: { essayId: string; metric: string; operation: string; declared: string; recalculated: string; confirmed: boolean; detail: string }[];
  review: { choice: string; finalDecision: string; justification: string; analystName: string; decidedAt: string } | null;
  audit: { at: string; message: string }[];
  activities: { id: string; cycle: string; phase: string; declaredNature: string; description: string; output: string; evidenceIds: string[] }[];
  evidences: { id: string; type: string; file: string; status: string; note: string }[];
};

export type ProjectDetail = {
  id: string;
  title: string;
  group: string;
  team: string;
  durationWeeks: number;
  activityCount: number;
  evidenceCount: number;
  warningCount: number;
  warnings: string[];
  latestAnalysis: Analysis | null;
};

export type Report = { title: string; sections: { title: string; body: string }[] };

export type Calibration = {
  projects: number;
  correctClassifications: number;
  criteria: { criterion: string; correct: number; total: number }[];
  divergences: { projectId: string; expected: string; actual: string }[];
};

async function request<T>(url: string, init?: RequestInit): Promise<T> {
  const response = await fetch(url, {
    headers: { "Content-Type": "application/json" },
    ...init
  });
  if (!response.ok) {
    const body = await response.json().catch(() => ({}));
    throw new Error(body.error ?? `Falha ${response.status}`);
  }
  return response.json() as Promise<T>;
}

export const api = {
  projects: () => request<ProjectListItem[]>("/api/projects"),
  project: (id: string) => request<ProjectDetail>(`/api/projects/${id}`),
  analyze: (id: string) => request<Analysis>(`/api/projects/${id}/analyses`, { method: "POST" }),
  review: (id: string, body: { choice: string; classification?: string; justification: string; analystName?: string }) =>
    request<Analysis>(`/api/analyses/${id}/review`, { method: "POST", body: JSON.stringify(body) }),
  report: (id: string) => request<Report>(`/api/projects/${id}/report`),
  calibrate: () => request<Calibration>("/api/calibration", { method: "POST" })
};

export function tone(label: string | null): string {
  switch (label) {
    case "Elegível":
      return "ok";
    case "Com ressalvas":
      return "warn";
    case "Não elegível":
      return "bad";
    case "Evidência insuficiente":
      return "gap";
    default:
      return "wait";
  }
}
