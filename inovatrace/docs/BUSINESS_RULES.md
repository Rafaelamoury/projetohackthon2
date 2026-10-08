# Regras de negócio

Fonte: `GUIA_DO_PARTICIPANTE.md`, `LEIA_ME.md`, `guia-do-desafio-hackathon-sts-2026.pdf` e o dicionário do pacote. O gabarito histórico não é regra de entrada.

## Cinco critérios

Novidade, criatividade, incerteza tecnológica, sistematicidade e transferibilidade/reprodutibilidade. São os cinco critérios do Manual de Frascati citados no guia. `natureza_informada_pela_equipe` é autodeclaração e não classifica.

## Quatro classificações

- **Elegível:** as evidências caracterizam P&D no escopo definido. Limite externo já excluído no início não vira ressalva sozinho. Sucesso comercial e ausência de falhas não são requisitos.
- **Com ressalvas:** há prova suficiente de P&D, mas uma limitação técnica ou de validação restringe parte da conclusão pretendida. Registrar o recorte, a limitação e a evidência que a resolveria. Não usar esta classe para tapar ausência essencial de prova.
- **Não elegível:** o mecanismo documentado é aplicação conhecida, configuração, integração, migração, manutenção ou aceite, sem investigação tecnológica. A conclusão decorre do mecanismo, não do título.
- **Evidência insuficiente:** falta o elo para distinguir P&D de rotina. Especificar o que falta.

## Evidências

Relacionar afirmação a arquivo por ID. Conferir versão, data, unidade e denominador. Separar plano de execução registrada. Manter visíveis evidências favoráveis, contrárias, ausentes e contraditórias.

`Localizada` significa arquivo presente, não alegação provada. Nesta massa os 560 itens estão localizados. Ausência, quando existe, é de conteúdo.

`resultados.csv` deriva de `medicoes.csv`. Repetir o número num PDF não confirma de novo. Contagem de entrega não mede desempenho. Taxa vazia não é zero.

## Ausente e contrária

Evidência contrária existe e aponta contra a conclusão pretendida. Evidência ausente é o elo que precisaria existir e não está no material. Contradição é o conflito entre duas fontes que existem, sem esconder nenhuma e sem resolvê-la no automático. O registro primário, com versão, prevalece sobre depoimento de memória quando os dois divergem.

## Entrevistas

Dão contexto. Não provam sozinhas. A pergunta "Como ficou a conclusão da rodada?" é cotejada com os valores recalculáveis.

## Projetos históricos

PRJ01–PRJ20 calibram o nível de fundamentação. A classe de um caso novo não é copiada por semelhança. O arquivo `historicos_classificados` só entra na calibração, depois que o motor já produziu o resultado.

## Rastreabilidade

Cada recomendação guarda critério, afirmação, atividade, evidência e texto. A recomendação da IA não é apagada quando o analista decide. A trilha registra importação, análise, recomendação, revisão e conclusão.

## Papel da IA e do analista

A IA interpreta o mecanismo descrito nos arquivos. O motor valida o que a conta fecha e aplica a combinação dos cinco estados. O analista decide. O sistema registra os dois.

Sem endpoint de IA configurado, a leitura usa frases explícitas do próprio `metodo.md` e da revisão: negação de mecanismo novo, falta essencial, hipótese com ponto não ensaiado, ou fronteira de escopo já excluída. Se nenhuma dessas leituras fecha, a classe é evidência insuficiente. A lista de frases está em `FrascatiRubric`.

## O que esta massa não pede

Segregação de horas, valor do incentivo e requisito fiscal da empresa.
