import { useEffect, useState } from "react";
import { Link, NavLink, Route, Routes, useNavigate, useParams } from "react-router-dom";
import { Analysis, api, Calibration, ProjectDetail, ProjectListItem, Report, tone } from "./api";

export function App() {
  return (
    <div className="shell">
      <aside className="side">
        <div className="brand">
          <span className="mark" aria-hidden="true">In</span>
          <div>
            InovaTrace
            <small>Lei do Bem</small>
          </div>
        </div>
        <nav>
          <NavLink to="/" end>Minha fila</NavLink>
          <NavLink to="/calibracao">Calibração</NavLink>
        </nav>
        <p className="principle">IA interpreta. O analista decide.</p>
      </aside>
      <main className="main">
        <Routes>
          <Route path="/" element={<QueuePage />} />
          <Route path="/projects/:id" element={<ProjectPage />} />
          <Route path="/projects/:id/review" element={<ReviewPage />} />
          <Route path="/projects/:id/parecer" element={<ReportPage />} />
          <Route path="/calibracao" element={<CalibrationPage />} />
        </Routes>
      </main>
    </div>
  );
}

function QueuePage() {
  const [items, setItems] = useState<ProjectListItem[] | null>(null);
  const [error, setError] = useState("");
  const [query, setQuery] = useState("");
  const [group, setGroup] = useState("Análise");
  const [status, setStatus] = useState("Todos");

  useEffect(() => {
    api.projects().then(setItems).catch((reason: Error) => setError(reason.message));
  }, []);

  const visible = (items ?? []).filter((item) => {
    const haystack = `${item.id} ${item.title} ${item.team} ${item.group}`.toLocaleLowerCase("pt-BR");
    if (query.trim() && !haystack.includes(query.trim().toLocaleLowerCase("pt-BR"))) return false;
    if (group !== "Todos" && item.group !== group) return false;
    if (status !== "Todos" && item.queueStatus !== status) return false;
    return true;
  });
  const inGroup = (items ?? []).filter((item) => group === "Todos" || item.group === group);
  const waiting = inGroup.filter((item) => item.queueStatus === "Aguardando análise").length;
  const review = inGroup.filter((item) => item.queueStatus === "Requer revisão").length;
  const done = inGroup.filter((item) => item.queueStatus === "Concluído").length;

  return (
    <>
      <h1>Minha fila</h1>
      <p className="lede">A fila abre nos casos para analisar. O histórico só entra pelo filtro e serve para calibrar; a classe dele não entra na leitura.</p>
      {error && <p className="error">{error}</p>}
      {items && (
        <div className="stats">
          <div className="stat"><strong>{inGroup.length}</strong><span>projetos</span></div>
          <div className="stat"><strong>{waiting}</strong><span>aguardando</span></div>
          <div className="stat"><strong>{review}</strong><span>em revisão</span></div>
          <div className="stat"><strong>{done}</strong><span>concluídos</span></div>
        </div>
      )}
      <div className="toolbar">
        <input type="search" placeholder="Buscar por código, título ou equipe" value={query} onChange={(event) => setQuery(event.target.value)} />
        <div className="filters">
          {["Todos", "Histórico", "Análise"].map((item) => (
            <button type="button" key={item} className={group === item ? "on" : ""} onClick={() => setGroup(item)}>{item}</button>
          ))}
        </div>
        <div className="filters">
          {["Todos", "Aguardando análise", "Requer revisão", "Concluído"].map((item) => (
            <button type="button" key={item} className={status === item ? "on" : ""} onClick={() => setStatus(item)}>{item === "Todos" ? "Qualquer estado" : item}</button>
          ))}
        </div>
      </div>
      {items === null && !error && <p className="lede">Carregando a fila…</p>}
      {items && visible.length === 0 && <p className="lede">Nenhum projeto com esse recorte.</p>}
      <div className="queue">
        {visible.map((item) => (
          <Link className="card" key={item.id} to={`/projects/${item.id}`}>
            <div>
              <div className="kicker">{item.id} · {item.group}</div>
              <strong>{item.title}</strong>
              <div className="muted">{item.team}</div>
            </div>
            <div className="status">
              {item.recommendation ? (
                <>
                  <div className={`pill ${tone(item.recommendation)}`}>{item.recommendation}</div>
                  <div className="kicker">{item.queueStatus}</div>
                </>
              ) : (
                <div className="pill wait">{item.queueStatus}</div>
              )}
            </div>
          </Link>
        ))}
      </div>
    </>
  );
}

function ProjectPage() {
  const { id = "" } = useParams();
  const [detail, setDetail] = useState<ProjectDetail | null>(null);
  const [error, setError] = useState("");
  const [busy, setBusy] = useState(false);
  const [criterion, setCriterion] = useState(0);

  async function load() {
    const project = await api.project(id);
    setDetail(project);
  }

  useEffect(() => {
    load().catch((reason: Error) => setError(reason.message));
  }, [id]);

  async function analyze() {
    setBusy(true);
    setError("");
    try {
      await api.analyze(id);
      await load();
    } catch (reason) {
      setError(reason instanceof Error ? reason.message : "Falha na análise");
    } finally {
      setBusy(false);
    }
  }

  const analysis = detail?.latestAnalysis ?? null;
  const selected = analysis?.criteria[criterion];
  return (
    <>
      <Link className="back" to="/">Fila</Link>
      <div className="kicker">{detail?.id ?? id}{detail ? ` · ${detail.group}` : ""}</div>
      <h1>{detail?.title ?? id}</h1>
      {detail && (
        <ul className="meta">
          <li>{detail.team}</li>
          <li>{detail.durationWeeks} semanas</li>
          <li>{detail.activityCount} atividades</li>
          <li>{detail.evidenceCount} evidências</li>
        </ul>
      )}
      {!detail && !error && <p className="lede">Carregando o projeto…</p>}
      {error && <p className="error">{error}</p>}
      {detail && detail.warnings.length > 0 && (
        <div className="panel spaced">
          <strong>Avisos do importador</strong>
          {detail.warnings.map((warning) => <p key={warning}>{warning}</p>)}
        </div>
      )}
      {detail && !analysis && (
        <div className="panel empty-analysis">
          <h2>Ainda sem leitura</h2>
          <p>O motor lê o método, as medições e a entrevista deste projeto. A classe histórica, quando existe, fica de fora.</p>
          <button className="primary" onClick={analyze} disabled={busy}>{busy ? "Analisando…" : "Executar análise"}</button>
        </div>
      )}
      {analysis && (
        <>
          <div className="decision">
            <section className={`banner ${tone(analysis.recommendation)}`}>
              <div className="kicker">Recomendação da IA</div>
              <h2>{analysis.recommendation}</h2>
              <p>{analysis.summary}</p>
              <p><strong>Limite:</strong> {analysis.limit}</p>
              {analysis.requiresHumanReview && <div className="pill warn">Requer revisão humana</div>}
            </section>
            <section className="panel">
              <div className="kicker">Decisão do analista</div>
              {analysis.review ? (
                <>
                  <h2>{analysis.review.finalDecision}</h2>
                  <p>{analysis.review.choice}. {analysis.review.justification}</p>
                  <p className="muted">{analysis.review.analystName}</p>
                </>
              ) : (
                <>
                  <h2>Pendente</h2>
                  <p>A recomendação acima permanece. A decisão final só existe depois da revisão.</p>
                </>
              )}
            </section>
          </div>
          <div className="grid">
            <div>
              <h2>Critérios</h2>
              {analysis.criteria.map((item, index) => (
                <button key={item.criterion} className={`criterion ${index === criterion ? "selected" : ""}`} onClick={() => setCriterion(index)}>
                  <strong>{mark(item.statusLabel)} {item.label}</strong>
                  <div>{item.statusLabel}</div>
                </button>
              ))}
              {selected && <Trace analysis={analysis} criterionIndex={criterion} />}
            </div>
            <div>
              <div className="panel">
                <h2>Evidências</h2>
                <div className="counts">
                  <div><strong>{analysis.counts.favorable}</strong><span>favoráveis</span></div>
                  <div><strong>{analysis.counts.contrary}</strong><span>contrárias</span></div>
                  <div><strong>{analysis.counts.contradictory}</strong><span>{analysis.counts.contradictory === 1 ? "contradição" : "contradições"}</span></div>
                  <div><strong>{analysis.counts.absent}</strong><span>ausentes</span></div>
                </div>
              </div>
              {analysis.contradictions.map((item) => (
                <div className="alert" key={item.explanation}>
                  <strong>Contradição · {item.affectedCriterion}</strong>
                  <div>{item.sourceA} × {item.sourceB}</div>
                  <p>{item.explanation}</p>
                </div>
              ))}
              {analysis.missing.map((item) => (
                <div className="panel" key={item.description}>
                  <strong>Lacuna · {item.affectedCriterion}</strong>
                  <p>{item.description}</p>
                  <p>{item.neededEvidence}</p>
                </div>
              ))}
              <div className="panel">
                <h2>Conferência matemática</h2>
                {analysis.mathChecks.length === 0 && <p className="muted">Nenhum ensaio numérico neste projeto.</p>}
                {analysis.mathChecks.map((check) => (
                  <p key={check.essayId} className={`math ${check.confirmed ? "confirmed" : "failed"}`}>
                    <span>{check.confirmed ? "Confirmado" : "Divergente"}</span>
                    {check.essayId}: {check.declared} → {check.recalculated}
                  </p>
                ))}
              </div>
            </div>
          </div>
          <div className="actions">
            <Link className="primary" to={`/projects/${id}/review`}>Revisar</Link>
            <Link className="ghost" to={`/projects/${id}/parecer`}>Gerar parecer</Link>
          </div>
        </>
      )}
    </>
  );
}

function mark(status: string) {
  if (status.startsWith("Não") || status.startsWith("Alegada") || status.startsWith("Insuficiente") || status.startsWith("Indeterminada") || status === "Parcial") return "⚠";
  if (status.includes("limite")) return "⚠";
  return "✓";
}

function Trace({ analysis, criterionIndex }: { analysis: Analysis; criterionIndex: number }) {
  const criterion = analysis.criteria[criterionIndex];
  const claim = analysis.claims.find((item) => item.code === criterion.claimCodes[0]);
  const activity = analysis.activities.find((item) => item.id === claim?.activityId);
  const evidence = analysis.evidences.filter((item) =>
    criterion.supportingEvidenceIds.includes(item.id) || criterion.contraryEvidenceIds.includes(item.id) || claim?.evidenceIds.includes(item.id));
  return (
    <div className="chain">
      <div className="step">
        <div className="kicker">{criterion.label}</div>
        <strong>{criterion.statusLabel}</strong>
        <p>{criterion.reasoning}</p>
      </div>
      <div className="arrow">↓</div>
      <div className="step">
        <div className="kicker">{claim?.code}</div>
        <p>{claim?.text}</p>
        <div className="kicker">Fonte: {claim?.source}</div>
      </div>
      {activity && (
        <>
          <div className="arrow">↓</div>
          <div className="step">
            <div className="kicker">{activity.id}</div>
            <strong>{activity.phase}</strong>
            <p>{activity.description}</p>
          </div>
        </>
      )}
      <div className="arrow">↓</div>
      {evidence.map((item) => (
        <div className="step" key={item.id}>
          <div className="kicker">{item.id} · {item.type}</div>
          <div>{item.file}</div>
          <div className="kicker">{item.note}</div>
        </div>
      ))}
    </div>
  );
}

function ReviewPage() {
  const { id = "" } = useParams();
  const navigate = useNavigate();
  const [detail, setDetail] = useState<ProjectDetail | null>(null);
  const [choice, setChoice] = useState("Concordar");
  const [classification, setClassification] = useState("Elegível");
  const [justification, setJustification] = useState("");
  const [analyst, setAnalyst] = useState("Analista");
  const [error, setError] = useState("");

  useEffect(() => {
    api.project(id).then(setDetail).catch((reason: Error) => setError(reason.message));
  }, [id]);

  const analysis = detail?.latestAnalysis;
  async function confirm() {
    if (!analysis) return;
    setError("");
    try {
      await api.review(analysis.id, { choice, classification: choice === "Alterar" ? classification : undefined, justification, analystName: analyst });
      navigate(`/projects/${id}`);
    } catch (reason) {
      setError(reason instanceof Error ? reason.message : "Falha ao confirmar");
    }
  }

  return (
    <>
      <Link className="back" to={`/projects/${id}`}>Projeto</Link>
      <h1>Revisão humana</h1>
      <p className="lede">{detail?.title ?? id}. A recomendação da IA permanece. A decisão final fica ao lado, sem sobrescrever.</p>
      {error && <p className="error">{error}</p>}
      {!detail && !error && <p className="lede">Carregando a análise…</p>}
      {analysis && (
        <div className="grid">
          <div className="panel">
            <div className="kicker">Recomendação da IA</div>
            <h2>{analysis.recommendation}</h2>
            <p>{analysis.summary}</p>
            <p><strong>Limite:</strong> {analysis.limit}</p>
          </div>
          <div className="panel">
            {analysis.review ? (
              <>
                <div className="kicker">Decisão registrada</div>
                <h2>{analysis.review.finalDecision}</h2>
                <p>{analysis.review.choice}. {analysis.review.justification}</p>
                <p className="muted">{analysis.review.analystName}</p>
                <div className="actions"><Link className="ghost" to={`/projects/${id}`}>Voltar ao projeto</Link></div>
              </>
            ) : (
              <>
                <div className="kicker">Decisão do analista</div>
                <div className="choice">
                  <label><input type="radio" checked={choice === "Concordar"} onChange={() => setChoice("Concordar")} /> Concordar</label>
                  <label><input type="radio" checked={choice === "Alterar"} onChange={() => setChoice("Alterar")} /> Alterar</label>
                </div>
                {choice === "Alterar" && (
                  <select value={classification} onChange={(event) => setClassification(event.target.value)}>
                    <option>Elegível</option>
                    <option>Com ressalvas</option>
                    <option>Não elegível</option>
                    <option>Evidência insuficiente</option>
                  </select>
                )}
                <label className="field">Analista</label>
                <input type="text" value={analyst} onChange={(event) => setAnalyst(event.target.value)} />
                <label className="field">Justificativa</label>
                <textarea rows={6} value={justification} onChange={(event) => setJustification(event.target.value)} placeholder="O que sustenta concordar ou alterar." />
                <div className="actions"><button className="primary" onClick={confirm} disabled={justification.trim().length === 0}>Confirmar</button></div>
              </>
            )}
          </div>
        </div>
      )}
      {analysis && (
        <div className="panel spaced">
          <h2>Trilha</h2>
          <ol className="timeline">
            {analysis.audit.map((entry) => (
              <li key={entry.at + entry.message}>
                <time>{new Date(entry.at).toLocaleString("pt-BR", { day: "2-digit", month: "2-digit", hour: "2-digit", minute: "2-digit" })}</time>
                <span>{entry.message}</span>
              </li>
            ))}
          </ol>
        </div>
      )}
    </>
  );
}

function ReportPage() {
  const { id = "" } = useParams();
  const [report, setReport] = useState<Report | null>(null);
  const [error, setError] = useState("");
  useEffect(() => {
    api.report(id).then(setReport).catch((reason: Error) => setError(reason.message));
  }, [id]);
  return (
    <>
      <div className="actions no-print">
        <Link className="back" to={`/projects/${id}`}>Projeto</Link>
        <button type="button" className="ghost" onClick={() => window.print()}>Imprimir</button>
      </div>
      {!report && !error && <p className="lede">Montando o parecer…</p>}
      <article className="report">
        <h1>{report?.title ?? "Parecer"}</h1>
        {error && <p className="error">{error}</p>}
        {report?.sections.map((section) => (
          <section key={section.title}>
            <h2>{section.title}</h2>
            <p>{section.body}</p>
          </section>
        ))}
      </article>
    </>
  );
}

function CalibrationPage() {
  const [result, setResult] = useState<Calibration | null>(null);
  const [error, setError] = useState("");
  const [busy, setBusy] = useState(false);

  async function run() {
    setBusy(true);
    setError("");
    try {
      setResult(await api.calibrate());
    } catch (reason) {
      setError(reason instanceof Error ? reason.message : "Falha na calibração");
    } finally {
      setBusy(false);
    }
  }

  return (
    <>
      <h1>Calibração</h1>
      <p className="lede">PRJ01 a PRJ20 rodam sem receber a classe histórica. A comparação acontece depois.</p>
      <button className="primary" onClick={run} disabled={busy}>{busy ? "Medindo…" : "Rodar PRJ01–PRJ20"}</button>
      {error && <p className="error">{error}</p>}
      {result && (
        <div className="panel spaced">
          <div className="score">{result.correctClassifications}<span>/{result.projects}</span></div>
          <p className="muted">classificações iguais à classe histórica, comparadas só depois da leitura.</p>
          <div className="meters">
            {result.criteria.map((item) => {
              const percent = item.total === 0 ? 0 : Math.round(100 * item.correct / item.total);
              return (
                <div key={item.criterion}>
                  <div className="meter-label"><span>{item.criterion}</span><strong>{item.total === 0 ? "—" : `${percent}%`}</strong></div>
                  <div className="meter" aria-hidden="true"><span style={{ width: `${percent}%` }} /></div>
                </div>
              );
            })}
          </div>
          <h3>Divergências</h3>
          {result.divergences.length === 0 && <p>Nenhuma.</p>}
          {result.divergences.map((item) => (
            <p key={item.projectId}>{item.projectId}: esperado {item.expected}, obtido {item.actual}</p>
          ))}
        </div>
      )}
    </>
  );
}
