# Análise do dataset — Hackathon STS 2026

Fonte técnica da massa do exercício Lei do Bem. Este documento descreve somente o que está nos arquivos. Não define regra de classificação além do que os próprios materiais enunciam, e não implementa código.

Leitura feita em 7 de outubro de 2026 sobre a cópia em `dados-originais/`. O pacote original em `Downloads` não foi alterado. Nenhum arquivo dentro de `dados-originais/` foi editado.

Pacote de origem: `HACKATHON STS 2026-pacote_participantes_lei_do_bem_v10` (versão documental 9.0; esquema de resultados 10.0).

## 1. Estrutura completa das pastas

```
dados-originais/
├── 00_dados_apoio/
│   ├── atividades_consolidadas.csv
│   ├── dicionario.pdf
│   ├── documentos.csv
│   ├── evidencias.csv
│   └── projetos.csv
├── 01_projetos/
│   ├── 01_historico/
│   │   └── PRJ01 … PRJ20
│   └── 02_casos_para_analise/
│       └── PRJ21 … PRJ40
├── Estrutura de um projeto.png
├── guia-do-desafio-hackathon-sts-2026.pdf
├── GUIA_DO_PARTICIPANTE.md
├── historicos_classificados.csv
├── historicos_classificados.md
├── LEIA_ME.md
└── Materiais.txt
```

Cada projeto, nos dois grupos, tem exatamente estes 14 arquivos:

```
PRJxx/
├── atividades.csv
├── atividades.xlsx
├── dossie_projeto.pdf
├── inventario_evidencias.csv
├── registro_tecnico.pdf
├── transcricao_entrevista_tecnica.pdf
└── evidencias/
    ├── configuracao.json
    ├── cronologia.csv
    ├── entradas.csv
    ├── medicoes.csv
    ├── metodo.md
    ├── observacoes.csv
    ├── resultados.csv
    └── revisao_tecnica.md
```

Há um único conjunto de arquivos entre os 40 projetos. Contagem no pacote: 285 CSV, 122 PDF, 83 Markdown, 40 XLSX, 40 JSON, 1 PNG, 1 TXT.

O PNG `Estrutura de um projeto.png` descreve o fluxo de leitura: dossiê, atividades, inventário, arquivos de evidência, entrevista e análise. Lembra que a classificação deve citar evidências, que a entrevista não prova sozinha, que `natureza_informada` é autodeclaração e que evidências favoráveis, contrárias e contraditórias importam.

## 2. Arquivos de apoio

| Arquivo | Função observada |
| --- | --- |
| `LEIA_ME.md` | Descrição técnica da massa: quatro classes, formatos, dicionário de `resultados.csv` e de `medicoes.csv`, relação com os índices. |
| `GUIA_DO_PARTICIPANTE.md` | Como classificar: cinco critérios, quatro classes, uso dos históricos como calibração, entrega esperada para PRJ21–PRJ40. |
| `guia-do-desafio-hackathon-sts-2026.pdf` | 7 páginas. Enunciado do desafio (apoio à decisão para a equipe interna de P&D do BNB), contexto da Lei do Bem, o que a banca olha e restrições do evento. |
| `00_dados_apoio/dicionario.pdf` | 23 páginas. Dicionário de identificadores, mapa EV→arquivo, campos, operações e vocabulário. Os exemplos numéricos do dicionário são didáticos e não são os resultados dos casos. |
| `00_dados_apoio/projetos.csv` | 40 linhas. Índice de projeto, grupo, equipe, duração, quantidades e pasta. |
| `00_dados_apoio/atividades_consolidadas.csv` | 320 linhas. União exata dos `atividades.csv` locais. Não acrescenta coluna de projeto; o projeto está no prefixo do ID. |
| `00_dados_apoio/evidencias.csv` | 560 linhas. União exata dos inventários locais. |
| `00_dados_apoio/documentos.csv` | 560 linhas. Mesma evidência, com `documento_id` e `caminho_no_pacote` relativos à raiz do pacote. |
| `historicos_classificados.csv` e `.md` | Classificação e justificativa somente de PRJ01–PRJ20. O Markdown está marcado como V9. |
| `Materiais.txt` | Três URLs: Lei nº 11.196/2005, guia da Lei do Bem (ANPEI/MCTIC) e Manual de Frascati. Os documentos em si não estão no pacote. |

Comparação índice × disco:

- 560 caminhos de `documentos.csv` existem.
- 560 arquivos citados nos inventários existem.
- 320 atividades consolidadas coincidem campo a campo com as locais.
- 560 evidências consolidadas coincidem campo a campo com os inventários.
- `documentos.csv` e o inventário concordam em `arquivo` e `tipo` nas 560 linhas.
- `projetos.csv` aponta para as 40 pastas existentes. `quantidade_atividades` é 8 e `quantidade_evidencias` é 14 em todos.

## 3. Estrutura dos projetos PRJ01 a PRJ40

Unidade de análise: um projeto (`PRJxx`).

Conteúdo fixo por projeto:

- 8 atividades (`ATV01`–`ATV08`), na mesma sequência de fase e de natureza informada.
- 14 evidências (`EV01`–`EV14`), com o mesmo arquivo, tipo, conteúdo esperado e observação.
- 1 dossiê de 1 página, 1 registro técnico de 1 página, 1 entrevista de 1 página.
- Método em Markdown (seções 1–7; quatro projetos têm seção 8).
- Revisão técnica em Markdown.
- Configuração JSON, cronologia, medições, resultados, entradas e observações.

O que varia é o conteúdo narrativo, os parâmetros, o número de linhas das séries e, em poucos projetos, chaves extras de JSON ou o formato interno de `entradas.csv`.

`projetos.csv` traz `titulo`, `grupo` (`Histórico` ou `Análise`), `equipe` (40 valores distintos), `duracao_semanas` (27 valores distintos) e `pasta`.

Amostras lidas por completo no texto extraível: PRJ01 (dossiê, registro técnico, entrevista, método, revisão, configuração, medições, resultados, entradas, cronologia, atividades, inventário) e o dossiê de PRJ21. Os demais projetos foram varridos por schema, contagem, IDs e chaves, não por leitura integral da narrativa.

## 4. Formatos de arquivos

| Formato | Onde | Convenção verificada |
| --- | --- | --- |
| CSV | índices, atividades, inventário, cronologia, medições, resultados, entradas, observações, históricos | UTF-8 com BOM em todos os CSV do pacote. Separador `;`. Aspas duplas quando o campo contém `;`. Aspas internas escapadas por duplicação. Decimal com ponto (`66.666667`). Vazio não é zero. |
| XLSX | `atividades.xlsx` | Uma planilha. Linhas 1–3 são título e aviso. Linha 4 vazia. Cabeçalho humano na linha 5. Dados a partir da linha 6. Nos 40 arquivos o cabeçalho é idêntico e as 8 linhas de dados coincidem, célula a célula, com `atividades.csv`. |
| JSON | `evidencias/configuracao.json` | UTF-8. Objeto único. `versao_documental` = `9.0`. `versao_esquema_resultados` = `10.0`. |
| Markdown | método, revisão, guias, históricos | Títulos `##` numerados no método. Âncoras usadas nas referências são o número da seção (`metodo.md#1`), não um id HTML. |
| PDF | dossiê, registro técnico, entrevista, guia, dicionário | Texto extraível. Dossiê, registro e entrevista têm 1 página e o carimbo “Massa inteiramente fictícia \| V9”. O registro técnico repete números que já estão em `resultados.csv`; o `LEIA_ME.md` diz que essa repetição não é confirmação independente. PDFs usam vírgula decimal na prosa (`66,6667%`). |
| PNG | mapa de navegação | Ilustração do fluxo. Não é dado de projeto. |

Não há coluna de horas nem de despesas em nenhum CSV.

## 5. Schemas e colunas dos CSVs

### `projetos.csv` — 40 linhas

`projeto_id`; `titulo`; `grupo`; `equipe`; `duracao_semanas`; `quantidade_atividades`; `quantidade_evidencias`; `pasta`

`grupo`: `Histórico` (20), `Análise` (20).

### `atividades.csv` e `atividades_consolidadas.csv` — 8 linhas por projeto, 320 no total

`id_atividade`; `ciclo`; `fase`; `natureza_informada_pela_equipe`; `descricao`; `resultado_ou_saida`; `evidencias_relacionadas`; `responsavel_por_funcao`

Nenhum campo vazio.

Fase, em ordem fixa nos 40 projetos:

1. Caracterização do problema
2. Confronto com referência anterior
3. Especificação do mecanismo
4. Preparação dos cenários
5. Registro de medições
6. Análise de ocorrências
7. Conferência dos resultados
8. Consolidação técnica

`natureza_informada_pela_equipe`, na mesma ordem, nos 40 projetos: Análise, Análise, Desenvolvimento, Documentação, Teste, Análise, Teste, Documentação. O guia do participante diz que essa coluna é autodeclaração e tem a mesma distribuição em todos os projetos, então não distingue classes.

`ciclo` em 39 projetos: `C1`, `C1`, `C1`, `C2`, `C2`, `C3`, `C3`, `C4`. Exceção única: PRJ31 usa `fixo-C1-v1`, `fixo-C1-v1`, `fixo-C1-v1`, `C2`, `C2`, `condicionado-C3-v2`, `condicionado-C3-v2`, `C4`.

`evidencias_relacionadas`: um ID, ou vários IDs separados por ` | ` (espaço, barra vertical, espaço).

O XLSX usa rótulos humanos na linha 5: “ID da atividade”, “Ciclo”, “Fase”, “Natureza informada”, “Descrição”, “Resultado ou saída”, “Evidências relacionadas”, “Responsável por função”. Os valores são os mesmos do CSV.

### `inventario_evidencias.csv` e `evidencias.csv` — 14 linhas por projeto, 560 no total

`id_evidencia`; `projeto_id`; `tipo`; `arquivo`; `conteudo_esperado`; `status`; `observacao`

`status` é `Localizada` nas 560 linhas. O `LEIA_ME.md` define isso como “o arquivo está presente”, não como “a alegação foi comprovada”.

Mapa estável EV → arquivo (o prefixo `PRJxx-` muda; o sufixo não):

| Sufixo | tipo | arquivo | observacao |
| --- | --- | --- | --- |
| EV01 | Dossiê | `dossie_projeto.pdf` | síntese |
| EV02 | Registro técnico | `registro_tecnico.pdf` | síntese derivada |
| EV03 | Atividades | `atividades.xlsx` | registro de atividades |
| EV04 | Inventário | `inventario_evidencias.csv` | índice |
| EV05 | Configuração | `evidencias/configuracao.json` | especificação |
| EV06 | Método | `evidencias/metodo.md` | especificação |
| EV07 | Cronologia | `evidencias/cronologia.csv` | registro de versões |
| EV08 | Medições | `evidencias/medicoes.csv` | registro primário sintético |
| EV09 | Resultados | `evidencias/resultados.csv` | calculado ou transcrito de EV08, conforme operação |
| EV10 | Entrevista | `transcricao_entrevista_tecnica.pdf` | depoimento |
| EV11 | Observações | `evidencias/observacoes.csv` | recortes sintéticos |
| EV12 | Entradas | `evidencias/entradas.csv` | recortes sintéticos |
| EV13 | Revisão técnica | `evidencias/revisao_tecnica.md` | revisão |
| EV14 | Atividades CSV | `atividades.csv` | derivado de EV03 |

### `documentos.csv` — 560 linhas

`documento_id`; `projeto_id`; `id_evidencia`; `arquivo`; `caminho_no_pacote`; `tipo`

Um documento por evidência. Exemplo: `PRJ01-DOC01` → `PRJ01-EV01` → `01_projetos/01_historico/PRJ01/dossie_projeto.pdf`. O `documento_id` não aparece nas atividades.

### `evidencias/cronologia.csv` — 211 linhas

`evento_id`; `data`; `versao`; `evento`; `estado`; `fonte`

4 a 7 linhas por projeto (4×11, 5×14, 6×8, 7×7). `estado` é sempre `registrado`. `data` é `AAAA-MM-DD`.

`evento` tem 4 valores:

- Registro do problema e das referências anteriores
- Preparação do recorte e critérios descritos no método
- Arquivamento dos registros desta versão
- Consolidação do limite de conclusão e pendências

`fonte` tem 4 valores: `evidencias/metodo.md#1`, `evidencias/metodo.md#3`, `evidencias/medicoes.csv`, `evidencias/revisao_tecnica.md`.

### `evidencias/medicoes.csv` — 2.056 linhas

`registro_id`; `ensaio_id`; `versao`; `cenario`; `tipo`; `metrica`; `valor`; `numerador`; `denominador`; `peso`; `unidade`

De 1 a 1.088 linhas por projeto. `tipo`: `contador` (1.844), `medicao` (204), `histograma` (8). `unidade`: `casos`, `s`, `min`, `ms`, `%`, `0-1`. Há 78 métricas distintas; o nome descreve o que foi contado e não diz, sozinho, se maior é melhor.

Campos vazios por tipo, sem exceção nas 2.056 linhas:

| tipo | valor | numerador | denominador | peso |
| --- | --- | --- | --- | --- |
| contador | vazio | preenchido | preenchido | vazio |
| medicao | preenchido | vazio | vazio | vazio |
| histograma | preenchido | vazio | vazio | preenchido |

### `evidencias/resultados.csv` — 139 linhas

`ensaio_id`; `versao`; `metrica`; `operacao`; `valor`; `base_de_calculo`; `descricao_base`; `taxa_percentual`; `unidade`; `fonte`; `natureza`

De 1 a 8 linhas por projeto. Uma linha por ensaio. `operacao`: `contagem` (123), `media` (5), `diferenca_maior_menor` (3), `mediana` (2), `valor_observado` (2), `indicador_precalculado` (2), `percentil_95` (2). `natureza`: `desempenho` (131), `entrega` (8). `unidade` inclui `pontos percentuais`, que não aparece em `medicoes.csv`.

`taxa_percentual` vazio em 24 linhas, nesta partição exata:

- `contagem` + `desempenho`: 115 preenchidas, 0 vazias
- `contagem` + `entrega`: 8 vazias
- qualquer outra operação: vazia

`fonte` tem sempre a forma `evidencias/medicoes.csv#<ensaio_id>`.

Ensaios `entrega`: PRJ08-S01, PRJ10-S01, PRJ17-S01, PRJ28-S01, PRJ32-S01, PRJ33-S01, PRJ35-S02, PRJ35-S03.

O significado de cada coluna e de cada operação está reproduzido em `LEIA_ME.md` e, de forma idêntica, em `dicionario_resultados` e `operacoes_resultados` dos 40 JSON.

### `evidencias/entradas.csv` — 208 linhas

`entrada_id`; `conteudo_json`

`conteudo_json` é uma string JSON. Dentro de cada projeto as chaves são estáveis. Entre projetos, não são.

32 projetos (2 ou 3 linhas; 71 objetos) usam exatamente `entrada` e `referencia`.

Oito projetos usam outro objeto:

| Projeto | Linhas | Chaves de `conteudo_json` |
| --- | --- | --- |
| PRJ08 | 6 | `id`, `timestamp`, `origem`, `tcp`, `http`, `causa_referencia`, `classe_produzida` |
| PRJ10 | 8 | `id`, `evento`, `sessao`, `estado_produzido`, `versao_maquina` |
| PRJ17 | 6 | `id`, `renda_sintetica`, `comprometimento`, `politica_antiga`, `politica_nova`, `decisao_antiga`, `decisao_nova` |
| PRJ27 | 46 | `id`, `pergunta`, `trecho_referencia`, `trechos_recuperados`, `relevancia_anotada`, `versao_recuperador`, `resposta_gerada`, `julgamento_resposta`, `versao_gerador` |
| PRJ28 | 10 | `id`, `convenio`, `vigencia_fim`, `canal`, `registro_convertido`, `versao_conversor` |
| PRJ32 | 8 | `id`, `dispositivo`, `modelo`, `sistema`, `primeira_aparicao`, `reputacao`, `decisao` |
| PRJ33 | 12 | `id`, `origem`, `timestamp`, `correlation_id`, `timestamp_corrigido` |
| PRJ35 | 41 | `id`, `noite`, `data`, `duracao_real_min`, `volume_final`, `previsao_anotada_min`, `hora_emissao`, `versao_modelo`, `corte_treino` |

O guia diz que entradas e observações são recortes e não representam a população inteira.

### `evidencias/observacoes.csv` — 89 linhas

`observacao_id`; `entrada`; `referencia`; `saida_ou_situacao`; `escopo`

2 linhas em 31 projetos e 3 em 9. Nenhum campo vazio. `escopo` é sempre: `recorte ilustrativo identificado; não representa a totalidade das medições`.

### `historicos_classificados.csv` — 20 linhas

`projeto_id`; `titulo`; `classificacao`; `justificativa`; `limite`; `fontes_decisivas`; `divergencia_depoimento`; depois cinco blocos `criterio_n`, `estado_n`, `justificativa_n`, `fonte_n` para n = 1..5.

Detalhe na seção 10.

## 6. Schemas dos JSONs

Há um único tipo de JSON: `evidencias/configuracao.json`.

Chaves presentes nos 40 projetos, nesta ordem na maioria:

`projeto_id`, `natureza`, `versao_documental`, `escopo`, `parametros`, `versoes_registradas`, `dicionario_medicoes`, `ensaios`, `dicionario_resultados`, `versao_esquema_resultados`, `operacoes_resultados`

Valores idênticos nos 40:

- `natureza`: `registros integralmente sintéticos de um exercício documental`
- `versao_documental`: `9.0`
- `versao_esquema_resultados`: `10.0`
- `dicionario_medicoes`, `dicionario_resultados` e `operacoes_resultados`: o mesmo objeto em todos. São o dicionário do `LEIA_ME.md`, não parâmetros do projeto.

`escopo` e `versoes_registradas` são texto e lista de versões próprios de cada projeto.

`parametros` é um objeto. Há 38 conjuntos de chaves e 113 chaves na união. A interseção entre todos os projetos é vazia. Não existe schema comum de parâmetros. Exemplos: PRJ01 tem `retencao_inicial_s` e `retencao_final_s`; PRJ21 tem `limiar_inicial`, `limiar_final` e `janela_chamadas`.

`ensaios` é uma lista. Cada item tem `ensaio_id`, `versao`, `metrica`, `tipo`, `operacao`. O conjunto de `ensaio_id` do JSON é igual ao de `resultados.csv` nos 40 projetos.

`ensaios[].tipo` não usa o vocabulário de `medicoes.csv`:

| `ensaios[].tipo` | quantidade de ensaios | `operacao` coberta |
| --- | --- | --- |
| `count` | 123 | `contagem` |
| `value` | 14 | `valor_observado`, `mediana`, `media`, `diferenca_maior_menor`, `indicador_precalculado` |
| `histogram` | 2 | `percentil_95` |

`medicoes.csv` usa `contador`, `medicao` e `histograma` na linha, não no ensaio. Não há igualdade de string entre os dois campos `tipo`.

Sete projetos acrescentam chaves no topo, fora do miolo comum:

| Projeto | Chaves extras |
| --- | --- |
| PRJ16 | `grafos_de_eventos`, `cenarios_falha_bloqueio` |
| PRJ24 | `grafos_de_eventos` |
| PRJ31 | `ataques_nao_bloqueados_C3` |
| PRJ34 | `pares_candidatos`, `relacoes_indevidas_P2`, `mudancas_nao_detectadas_P2` |
| PRJ37 | `regra_selecao`, `registro_treinamentos_sinteticos`, `versao_entrega_selecionada`, `identificador_modelo_selecionado` |
| PRJ38 | `catalogo_148_regras`, `atrasos_ms_por_perfil`, `agregacao` |
| PRJ40 | `matriz_ausencias`, `padrao_pendente` |

O importador deve preservar essas chaves. Elas não foram interpretadas aqui.

## 7. Relacionamentos por IDs

Prefixos observados:

| ID | Exemplo | Liga |
| --- | --- | --- |
| `PRJxx` | `PRJ01` | projeto; prefixo de todos os IDs locais |
| `PRJxx-ATVnn` | `PRJ01-ATV03` | atividade |
| `PRJxx-EVnn` | `PRJ01-EV08` | evidência / arquivo |
| `PRJxx-DOCnn` | `PRJ01-DOC08` | documento no índice; paralelo à evidência, não usado nas atividades |
| `PRJxx-CRnn` | `PRJ01-CR03` | evento de cronologia |
| `PRJxx-Snn` | `PRJ01-S02` | ensaio |
| `PRJxx-Snn-Mnnn` | `PRJ01-S02-M001` | linha de medição |
| `PRJxx-OBSnn` | `PRJ01-OBS01` | observação |
| `PRJxx-INnnn` | `PRJ01-IN001` | entrada |

Junções que fecham sem órfão:

- Atividade → evidência, pelos IDs em `evidencias_relacionadas`. 0 referências quebradas.
- Evidência → arquivo, pela coluna `arquivo`, relativo à pasta do projeto. 0 arquivos ausentes.
- Documento → evidência → caminho no pacote. 1 para 1. 0 caminhos ausentes.
- `resultados.ensaio_id` = `medicoes.ensaio_id` = `configuracao.ensaios[].ensaio_id`. 0 ensaios só de um lado.
- `resultados.fonte` seleciona as linhas daquele `ensaio_id` em `medicoes.csv`.
- Cronologia `fonte` aponta para seção 1 ou 3 do método, para o arquivo de medições ou para a revisão. Não aponta para ensaio.
- Histórico `projeto_id` cobre PRJ01–PRJ20 e nenhum caso de análise.
- `atividades.csv` é EV14 e o inventário o declara derivado de EV03 (`atividades.xlsx`). A comparação célula a célula confirma a igualdade.

Âncora `arquivo#identificador`: o trecho depois de `#` é seção ou ensaio, não outro arquivo. Visto em `resultados.fonte`, em `cronologia.fonte` e nas colunas `fonte_n` do histórico (`evidencias/metodo.md#1`, `#2`, `#3`, `evidencias/medicoes.csv`, `evidencias/revisao_tecnica.md`).

## 8. Como Activity referencia Evidence

O padrão é o mesmo nos 40 projetos. A atividade cita o sufixo; o ID completo leva o prefixo do projeto.

| Atividade | Fase | Evidências |
| --- | --- | --- |
| ATV01 | Caracterização do problema | EV01 dossiê |
| ATV02 | Confronto com referência anterior | EV06 método |
| ATV03 | Especificação do mecanismo | EV05 configuração, EV06 método |
| ATV04 | Preparação dos cenários | EV05, EV06, EV12 entradas |
| ATV05 | Registro de medições | EV08 medições |
| ATV06 | Análise de ocorrências | EV11 observações |
| ATV07 | Conferência dos resultados | EV08 medições, EV09 resultados |
| ATV08 | Consolidação técnica | EV07 cronologia, EV13 revisão |

Evidências que existem e não são citadas por nenhuma atividade: EV02 registro técnico, EV04 inventário, EV10 entrevista, EV14 atividades CSV. Isso não significa arquivo ausente. A entrevista entra pelo inventário e pelo dossiê (“a entrevista é um depoimento e pode divergir dos registros”), não pela coluna `evidencias_relacionadas`.

Não há, na massa, uma entidade Claim. A atividade é o registro da equipe sobre uma parte do trabalho, com descrição, saída declarada e evidências ligadas. Afirmações avaliáveis mais finas do que a atividade não estão pré-extraídas.

## 9. Diferenças entre PRJ01–20 e PRJ21–40

O que separa os grupos, de forma explícita nos arquivos:

| | PRJ01–PRJ20 | PRJ21–PRJ40 |
| --- | --- | --- |
| Pasta | `01_projetos/01_historico` | `01_projetos/02_casos_para_analise` |
| `projetos.grupo` | `Histórico` | `Análise` |
| Classificação publicada | sim, só em `historicos_classificados.csv` e `.md` | não há classe, dificuldade nem gabarito |
| Conjunto de 14 arquivos | igual | igual |
| Schemas CSV, fases, naturezas, mapa EV | iguais | iguais |

O que não separa os grupos: tamanho de PDF, presença de medição, status do inventário, número fixo de atividades. Linhas de medição, resultados, cronologia e entradas variam dentro dos dois grupos. Chaves extras de JSON e a seção 8 do método aparecem nos dois lados (PRJ03 e PRJ16 são históricos; PRJ24, PRJ27, PRJ31, PRJ34, PRJ35, PRJ37, PRJ38 e PRJ40 são casos).

O guia diz que PRJ01–PRJ20 servem para calibrar o nível de fundamentação, e que a classe de um caso novo não deve ser copiada por semelhança com um histórico.

## 10. Arquivos com classificação histórica

Somente estes dois, na raiz do pacote:

- `historicos_classificados.csv`
- `historicos_classificados.md`

Não há classificação dentro da pasta do projeto. PRJ21–PRJ40 não aparecem nesses arquivos.

Classes (20 linhas): Não elegível 7, Elegível 6, Com ressalvas 4, Evidência insuficiente 3.

| projeto_id | classificacao | `divergencia_depoimento` preenchida |
| --- | --- | --- |
| PRJ01 | Não elegível | não |
| PRJ02 | Elegível | sim |
| PRJ03 | Elegível | não |
| PRJ04 | Não elegível | não |
| PRJ05 | Com ressalvas | sim |
| PRJ06 | Com ressalvas | não |
| PRJ07 | Com ressalvas | sim |
| PRJ08 | Evidência insuficiente | não |
| PRJ09 | Não elegível | não |
| PRJ10 | Evidência insuficiente | não |
| PRJ11 | Não elegível | não |
| PRJ12 | Não elegível | não |
| PRJ13 | Elegível | sim |
| PRJ14 | Elegível | não |
| PRJ15 | Elegível | sim |
| PRJ16 | Elegível | não |
| PRJ17 | Evidência insuficiente | não |
| PRJ18 | Com ressalvas | sim |
| PRJ19 | Não elegível | não |
| PRJ20 | Não elegível | não |

Critérios e estados publicados (os rótulos são os da coluna, não uma normalização):

| n | `criterio_n` | estados observados |
| --- | --- | --- |
| 1 | Novidade | NÃO DEMONSTRADA (7), DEMONSTRADA NO RECORTE (10), INDETERMINADA (3) |
| 2 | Criatividade técnica | NÃO DEMONSTRADA (7), DEMONSTRADA NO RECORTE (10), INDETERMINADA (3) |
| 3 | Incerteza tecnológica | NÃO CARACTERIZADA (7), INVESTIGADA (10), ALEGADA, NÃO VERIFICÁVEL (3) |
| 4 | Sistematicidade | DOCUMENTADA COMO ACEITE (7), DOCUMENTADA (10), PARCIAL (3) |
| 5 | Transferência/reprodução | DOCUMENTADA PARA A CONFIGURAÇÃO (7), DOCUMENTADA NO ESCOPO (6), DOCUMENTADA COM LIMITE (4), INSUFICIENTE PARA O NÚCLEO ALEGADO (3) |

`fontes_decisivas` é a mesma string nas 20 linhas: `evidencias/metodo.md | evidencias/medicoes.csv | evidencias/revisao_tecnica.md`.

`fonte_1` é sempre `evidencias/metodo.md#1`. As outras fontes por critério apontam para método, medições ou revisão, conforme a linha. O Markdown repete a justificativa em prosa e não acrescenta projeto fora do CSV.

Os três históricos de evidência insuficiente são PRJ08, PRJ10 e PRJ17. São também três dos oito projetos cujo `entradas.csv` não usa o par `entrada`/`referencia`.

Para calibração, esta classificação é gabarito externo. O guia pede que a análise do próprio projeto não receba a resposta histórica como entrada.

## 11. Informações determinísticas

Dá para resolver por leitura ou por conta, sem interpretar se o projeto é P&D:

- Identidade do projeto, pasta, equipe, duração, título e grupo.
- Inventário: ID, tipo, caminho, status `Localizada`.
- Ligação atividade → evidência → arquivo, no padrão da seção 8.
- Igualdade entre índices consolidados e arquivos locais.
- Igualdade entre `atividades.xlsx` e `atividades.csv`.
- Junção de ensaio entre JSON, medições e resultados.
- Seleção das linhas de medição pelo `ensaio_id` citado em `resultados.fonte`.
- Preenchimento estrutural de `valor`, `numerador`, `denominador` e `peso` conforme `medicoes.tipo`.
- Preenchimento de `taxa_percentual`: presente só em contagem de desempenho; vazio em entrega e nas outras operações. O `LEIA_ME.md` define a conta da taxa de contagem como `100 × valor / base_de_calculo`.
- Operações nomeadas no dicionário, quando a medição traz os insumos: somar numerador e denominador na contagem; média; mediana; maior menos menor; percentil 95 do histograma; copiar `valor_observado`; conferir transcrição de `indicador_precalculado` sem refazer a conta original.
- Datas, versões e estado `registrado` da cronologia.
- Texto integral de método, revisão, dossiê, registro técnico e entrevista, como fonte citável.
- Para PRJ01–PRJ20, a classe, o estado de cada critério e a divergência de depoimento já publicada, lidos do gabarito e não inferidos.

“Localizada”, contagem de linhas e existência de anexo não comprovam a alegação. O `LEIA_ME.md` também diz que contagem de entrega não mede desempenho, e que um resultado desfavorável pode ser P&D e um aceite perfeito pode ser rotina.

## 12. Informações que exigem interpretação

Não estão calculadas nos arquivos dos casos PRJ21–PRJ40, e nos históricos já vêm prontas só no gabarito:

- Se o trabalho é investigação tecnológica ou aplicação, configuração, integração, migração ou verificação de técnica já descrita.
- Os cinco critérios no sentido do Manual de Frascati, citados pelo guia: novidade, criatividade, incerteza tecnológica, sistematicidade, transferibilidade ou reprodutibilidade.
- A classe: Elegível, Com ressalvas, Não elegível, Evidência insuficiente.
- O recorte sustentado, a limitação e a evidência que resolveria uma ressalva.
- O elo ausente, quando a prova não chega a distinguir P&D de rotina.
- Se um número é favorável ou contrário à conclusão pretendida. A métrica não carrega esse sinal.
- Contradição entre entrevista e registro. O depoimento está no PDF. Nos históricos, seis projetos têm a divergência já redigida; nos outros quatorze o campo está vazio, o que não dispensa a leitura da entrevista.
- O que é limite de escopo declarado desde o início e o que é lacuna da prova.
- O significado dos parâmetros e das chaves extras de JSON no mecanismo daquele projeto.
- Afirmações mais finas que a atividade. A massa não traz Claim pronto.

O dossiê de PRJ01 e o de PRJ21 seguem o mesmo esqueleto: contexto, pergunta registrada, referência anterior, trabalho documentado, limite da conclusão, localização da prova. A entrevista de PRJ01 tem sete perguntas fixas no texto lido e fecha com “Depoimento de memória. Confrontar com versões, critérios, resultados e fontes entregues.” Não foi verificada a igualdade dessas sete perguntas nos outros 39 PDFs.

## 13. Inconsistências, campos opcionais e dúvidas

Não há arquivo prometido e ausente, nem ID de atividade apontando para evidência inexistente, nem ensaio sem medição. As variações abaixo são reais e não devem ser normalizadas por suposição.

1. `parametros` não tem schema compartilhado. Tratar como objeto aberto.
2. Sete JSON têm chaves de topo fora do miolo comum. Preservar e não inventar semântica.
3. `entradas.csv` tem dois níveis: colunas fixas, e JSON interno com 9 formatos. O formato usual é `{entrada, referencia}`.
4. `ensaios[].tipo` (`count`, `value`, `histogram`) e `medicoes.tipo` (`contador`, `medicao`, `histograma`) não são a mesma enumeração. A correspondência da seção 6 é contagem observada, não um de-para declarado no arquivo.
5. PRJ31 nomeia ciclos com rótulos de versão (`fixo-C1-v1`, `condicionado-C3-v2`). `C1`–`C4` não são o único vocabulário.
6. `metodo.md` tem seções 1–7 em 36 projetos. Seção 8 adicional: PRJ03 “Leitura dos valores únicos”, PRJ27 “Catálogo documental fictício recuperado”, PRJ35 “Trecho recuperado do notebook”, PRJ37 “Leitura dos indicadores registrados”.
7. `revisao_tecnica.md` tem cinco seções em 39 projetos. PRJ27 acrescenta “Unidades de comparação”.
8. Vazios de medição e de taxa seguem o tipo e a operação. Não são dado faltante aleatório. Vazio não é zero.
9. `divergencia_depoimento` está vazio em 14 dos 20 históricos. Campo opcional no gabarito.
10. Atividades não referenciam EV02, EV04, EV10 e EV14. O arquivo existe.
11. Não existe status de evidência diferente de `Localizada`. “Evidência ausente”, no sentido do exercício, não é um arquivo que falte nesta massa. É informação que o conteúdo não sustenta. Isso precisa ser confirmado como regra na próxima etapa, não assumido pelo importador.
12. Os nomes dos critérios no gabarito (“Criatividade técnica”, “Transferência/reprodução”) não são idênticos aos do guia do participante (“Criatividade”, “Transferibilidade ou reprodutibilidade”). Os estados do gabarito também não são as quatro classes do projeto. Não foram unificados aqui.
13. CSV usa ponto decimal; PDF e parte da prosa usam vírgula. Comparar número exige normalizar a representação, não o significado.
14. O registro técnico e o dossiê repetem trechos do método e números dos resultados. O pacote avisa que isso não é segunda medição.
15. `natureza_informada_pela_equipe` é igual em todos e não serve para classificar.
16. Os exemplos numéricos do `dicionario.pdf` não são medições dos projetos.

Dúvida deixada em aberto, de propósito: o importador de PRJ01 pode gravar atividades, evidências e documentos pelos IDs e caminhos acima sem inferência. Não deve promover o gabarito histórico a campo do projeto analisado, nem tratar parâmetro, chave extra ou JSON de entrada fora do par `entrada`/`referencia` como coluna fixa. PRJ01 em particular usa o par simples em `entradas.csv` e o miolo comum de JSON, com dois parâmetros numéricos.

## O que esta análise não faz

Não classifica PRJ21–PRJ40. Não recalcula os 139 resultados. Não extrai afirmações. Não altera a massa.
