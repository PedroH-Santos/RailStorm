# RailStorm — Documentação do Projeto

> **Instrução de manutenção (leia primeiro):** este arquivo é a referência oficial do projeto e deve ser consultado sempre que uma tarefa envolver o RailStorm. **Toda vez que uma alteração no projeto mudar uma regra, sistema, fluxo ou arquivo aqui descrito, este documento deve ser atualizado na mesma tarefa**, antes de considerar o trabalho concluído (nova seção para sistema novo, remoção do item correspondente de "Lacunas conhecidas" quando implementado, ajuste de regras de negócio quando uma fórmula/valor mudar, etc.). Não deixe a doc dessincronizar do código.
>
> **Formato obrigatório para cada funcionalidade** (seção 4 e qualquer sistema novo adicionado no futuro): a subseção deve abrir com um mini texto de **"O que é / ideia central"** — 1 a 3 frases explicando em linguagem simples o propósito da funcionalidade e o problema/experiência que ela resolve no jogo — seguido de **"Regras"** com as regras de negócio explícitas (valores, fórmulas, condições, limites), e só depois a lista técnica de onde ela está no código (arquivos/classes). Toda funcionalidade nova criada no projeto precisa ganhar essa mesma estrutura aqui, não só a lista de arquivos.
>
> **Sem comentários no código:** não adicione comentários (`//`, `/* */`, `///`) em código C# novo ou editado neste projeto. O código deve se explicar por nomes claros de variáveis/métodos; a explicação de propósito/regras de negócio vai na documentação (aqui no CLAUDE.md), não inline no arquivo. Mensagens de `Debug.Log`/`Tooltip`/`Header` de Inspector não são "comentários" e continuam permitidas normalmente.

## 1. Visão do jogo

### 1.1 High Concept

O jogo é um **action roguelike sobre trilhos**, no qual o jogador controla um vagão armado que percorre caminhos pré-definidos construídos estrategicamente ao longo de cada run. Em vez de liberdade total de movimento, o desafio está em **escolher quais rotas desbloquear**, **quando mudar a direção do vagão** e **como enfrentar ondas de inimigos** enquanto evolui seu personagem por meio de upgrades temporários e progressão permanente. Cada decisão influencia diretamente a sobrevivência, o acesso a áreas estratégicas do mapa e o ritmo da jogabilidade, culminando em batalhas contra chefes que encerram cada mapa.

### 1.2 Game Overview

Action game 3D isométrico com elementos de roguelike, no qual o jogador controla um vagão que se move exclusivamente sobre trilhos. Ao iniciar uma run, o jogador constrói/desbloqueia trilhos apenas em direções pré-definidas, limitado pela compra de novos caminhos com o dinheiro recebido ao eliminar inimigos. Essa limitação impede percorrer todo o mapa em uma única run, reforçando decisões estratégicas.

Durante o gameplay, o jogador muda a direção do vagão em tempo real, atira e luta contra diferentes tipos de inimigos, e sobrevive a ondas progressivamente mais difíceis. Inimigos variam entre unidades simples, atiradores à distância e adversários especiais capazes de criar armadilhas nos trilhos ou remover o jogador do vagão.

Ao derrotar inimigos, o jogador recebe moedas, usadas para comprar itens ou novos caminhos **durante a run**. Após eliminar todos os inimigos de uma wave, o jogador escolhe entre 3 novas habilidades (armas novas ou melhorias no personagem).

O mapa conta com locais especiais que oferecem vantagens: baús de itens, lojas, aprimoramento de armas e bônus especiais (ex.: mais moedas por um período). Após um tempo de gameplay, um **boss simples** surge para encerrar o mapa; ao derrotá-lo, o jogador desbloqueia novos mapas com dificuldade mais alta (até três mapas diferentes).

Fora das runs, o jogador usaria um segundo tipo de **moeda de meta-progressão** (obtida por quests/progresso geral) para desbloquear **novos personagens** (cada um com mecânica/upgrade exclusivo inicial) e **upgrades permanentes** entre partidas.

> **Nota:** a seção 6 ("Lacunas conhecidas") marca quais partes dessa visão já existem no código e quais ainda são só design.

## 2. Stack técnica

- **Unity + URP** (Universal Render Pipeline).
- **Unity Splines** (`UnityEngine.Splines`) — sistema de trilhos ativo. Os arquivos `Dreamteck.Splines*.csproj` na raiz do repo são resíduo de um pacote removido; **não há** pasta `Assets/Dreamteck` nem uso real do Dreamteck no projeto.
- **Unity Input System** (novo, não o legado).
- **NavMesh** — IA de inimigos (`NavMeshAgent`).
- **TextMeshPro** — toda a UI de texto.

## 3. Arquitetura e convenções observadas

- **ScriptableObjects para dados de design**: `WeaponDefinition`, `SkillDefinition`, `ItemDefinition`, `HordeEventConfig`, `ChestLootTable`, `RarityConfig`, `SplineManifest`. A maioria é carregada em runtime via `Resources.Load`/`Resources.LoadAll` a partir de `Assets/Resources/`.
- **`IDrawable`** (`Assets/Scripts/Skills/Interfaces/IDrawable.cs`) — interface comum a `SkillDefinition`, `WeaponDefinition` e `ItemDefinition`, usada para exibição unificada em cards de UI (loja, baú, seleção de habilidades, inventário).
- **Eventos C#** (`Action`/`event`, vários estáticos) para desacoplar sistemas — ex.: `EnemySpawner.OnWaveCleared/OnWaveStarted`, `ChestInteractable.OnChestOpened`, `HordeSpawner.OnHordeStarted/OnHordeEnded`. Não há um Event Bus central.
- **Singletons simples** (campo estático `Instance`, sem framework): `SplineRuntimeState`, `RarityConfig`, `InteractPromptUI`, `ChestRevealEffect`.
- **Convenção de Canvas de UI**: toda UI começa **desativada** na cena — sem exceção, inclusive as que são singletons (`InteractPromptUI`, `ChestRevealEffect`). Duas formas de ativação, dependendo de quem é o dono do ciclo de vida:
  - **UI aberta por um "dono" explícito** (`ShopUI`, `AbilitySelectionUI`): quem abre chama `root.SetActive(true)`/`gameBackground.SetActive(true)` diretamente.
  - **UI singleton chamada via `Instance` de qualquer lugar** (`InteractPromptUI`, `ChestRevealEffect`): a própria propriedade estática `Instance` se auto-ativa na primeira vez que é acessada — usa `FindFirstObjectByType<T>(FindObjectsInactive.Include)` pra achar o componente mesmo com o GameObject desativado, chama `SetActive(true)` nele (o que dispara o `Awake()` na hora, registrando a instância de verdade), e só então retorna. Isso evita depender do checkbox "Active" estar certo manualmente na cena — se alguém desativar o Canvas por engano, o primeiro `Instance.Show(...)` religa ele sozinho. Ao criar uma UI singleton nova desse tipo, copie esse padrão de `Instance` (não um campo estático simples).
- **Padrão "handler" por responsabilidade** no Player: `PlayerItemHandler`, `PlayerSkillHandler`, `PlayerCartWeaponHandler` — cada um gerencia aquisição/upgrade/exílio de um tipo de coisa e aplica efeitos.
- **Namespace `StarterAssets`** é usado de forma inconsistente — resíduo do pacote padrão da Unity (`Assets/StarterAssets/`), reaproveitado só em parte dos scripts (`PlayerController`, `PlayerStatsAggregator`, `PlayerSkillHandler`, `ShopManager`, entre outros).
- **Pastas/arquivos a ignorar ao explorar o projeto** (não são conteúdo do jogo):
  - `Assets/_Recovery/` — ~31 cenas `.unity` numeradas, snapshots de auto-recovery do Editor.
  - `Assets/TutorialInfo/` + `Assets/Readme.asset` — Readme padrão do template URP da Unity, conteúdo genérico não customizado.
  - `Dreamteck.Splines*.csproj` na raiz — projetos VS órfãos de pacote removido.

## 4. Sistemas implementados

### 4.1 Trilhos (Splines) — `Assets/Scripts/Splines/`

**O que é / ideia central:** é o coração do "roguelike sobre trilhos" — o vagão só se move sobre caminhos pré-definidos (splines), e o jogador escolhe em tempo real para qual caminho virar nas bifurcações. Alguns caminhos começam bloqueados e só ficam acessíveis se o jogador pagar para desbloqueá-los, o que força a escolher rotas em vez de explorar tudo numa run só.

**Regras:**
- Troca de direção só é aceita se o input do jogador apontar fortemente para a spline de saída (`Vector3.Dot >= 0.4`), com cooldown de `0.25s` entre trocas e limiar mínimo de input `0.15`.
- Splines bloqueadas nunca são escolhidas automaticamente na troca de direção.
- Desbloqueio de uma spline bloqueada custa moedas (`SplineManifest.unlockCost`, valor por spline) e é feito por um menu dedicado, não durante o movimento normal.
- O estado de bloqueio/desbloqueio é por run (`SplineRuntimeState`), refletido visualmente (trilho quebrado vs. normal).

- **`SplineTrackBuilder.cs`** (+ Editor) — ferramenta de editor que popula uma spline com pranchas (`plankPrefab`/`brokenPlankPrefab`) espaçadas uniformemente (`count = round(comprimentoTotal / comprimentoPeça)`).
- **`SplineCollision.cs`** — detecta troca de direção em bifurcações via `KnotLinkCollection`; escolhe a spline cuja direção de saída melhor casa com o input (`Vector3.Dot >= 0.4`). Regras: `switchCooldown = 0.25s`, `inputThreshold = 0.15`; ignora splines bloqueadas.
- **`SplineUnlockZone.cs`** — menu de desbloqueio (tecla E, pausa o jogo com `Time.timeScale = 0`), navegação por teclado/gamepad, gasta moedas (`SplineManifest.unlockCost`) para desbloquear via `SplineRuntimeState.Unblock`.
- **`SplineRuntimeState.cs`** — singleton runtime, `HashSet<int>` de splines desbloqueadas; evento `OnSplineUnblocked`.
- **`SplinePathVisual.cs`** — alterna visual normal/quebrado conforme bloqueio.
- **`Manifest/SplineManifest.cs`** — ScriptableObject com metadados por spline (nome, destino, custo, cor, ícone, `isBlockedByDefault`); botão editor "Populate from SplineContainer".
- **`Dto/SplineInfo.cs`, `Dto/SplineBlock.cs`** — DTOs auxiliares (`SplineBlock` parece redundante, não referenciado pelo `SplineRuntimeState`).

### 4.2 Player / Vagão — `Assets/Scripts/Player/`, `Assets/Scripts/Cart/`, `Assets/Scripts/CartWeapons/`

**O que é / ideia central:** representa o jogador e o vagão armado que ele controla. Cobre movimento sobre o trilho, vida/stats centrais, e as armas equipadas no vagão (até 3 simultâneas), que são o principal meio de dano contra as ondas de inimigos.

**Regras:**
- Velocidade do vagão interpola entre `IdleSpeed` (parado/sem input) e `MoveSpeed` (movendo), via aceleração/desaceleração configuráveis — não é instantâneo.
- O vagão comporta no máximo `maxWeapons = 3` armas equipadas ao mesmo tempo; ao tentar equipar uma 4ª, é preciso exilar/substituir uma existente.
- Stats do jogador (HP, MaxHP, MoveSpeed, Coins, LuckPercent) ficam centralizados em um único agregador, e HP/LuckPercent são sempre "clampados" dentro de limites válidos.
- Existe uma arma "principal" do personagem (mira e atira independente do vagão) que roda em paralelo às armas do vagão — hoje com stats fixos no código, não conectados ao agregador de stats.
- Só a arma do tipo **Arrow** está de fato jogável hoje; **Magic** existe só como dado de design (ver seção 6).

- **`PlayerController.cs`** (`namespace StarterAssets`) — move o jogador sobre a spline atual, interpola velocidade (`IdleSpeed`↔`MoveSpeed` via `Acceleration`/`Deceleration`), decide direção pelo dot do input com a tangente da spline. Expõe `SwitchToSplineIndex` e `SetMovementLocked`.
- **`PlayerInputReader.cs`** — wrapper do Input System, expõe `Move`.
- **`Stats/PlayerStatsAggregator.cs`** — hub central de stats: `HP`/`MaxHP` (clamp), `MoveSpeed`, `IdleSpeed`, `Coins` (nunca negativo), `LuckPercent` (clamp 0–100); registra `StatDescriptor`s para UI.
- **`Systems/LifeSystem.cs`** — vida genérica (player ou inimigo); usa `PlayerStatsAggregator.HP` se presente, senão vida local própria; dispara `OnDeath`.
- **Arma principal (paralela às armas do vagão)**: `WeaponController.cs` (mira mouse/gamepad, `bulletDamage=15`, `bulletSpeed=25`, `bulletRange=15`, cooldown fixo `0.3s`, não lê `PlayerStatsAggregator`) + `PlayerFireballController.cs` (projétil).
- **`Animations/PlayerAnimationController.cs`** — alterna 2 índices de animação de ataque.
- **`Items/PlayerItemHandler.cs`** — ver seção 4.6.
- **`Skills/PlayerSkillHandler.cs`** — ver seção 4.6.

**Armas do vagão (Cart):**
- **`Cart/PlayerCartWeaponHandler.cs`** — inventário de até `maxWeapons = 3` armas equipadas (`WeaponDefinition`); aquisição/upgrade/exílio; evento `OnWeaponsChanged`.
- **`Cart/Weapons/WeaponDefinition.cs`** — ScriptableObject de arma (`IDrawable`), níveis por raridade (`WeaponLevelData`), cache de stats efetivos aplicando `WeaponSkillDefinition`.
- **`Cart/Weapons/WeaponLevelData.cs`** — base abstrata (dano/cadência/alcance) + `ArrowLevelData` (speed, arrowCount) e `MagicLevelData` (area, castTime).
- **`Cart/Weapons/WeaponSkillDefinition.cs`** — skill que modifica um stat de um tipo de arma específico (3 níveis padrão: +10%/+20%/+30%).
- **`Cart/Wheels/WheelSpin.cs`** — puramente visual, gira a roda proporcional a `PlayerController.CurrentSpeed`.
- **`CartWeapons/Arrow/ArrowWeaponController.cs`** — **única arma implementada**: dispara leque de flechas dos dois lados do vagão (`leftFirePoint`/`rightFirePoint`), `arrowCount` distribuídas com margem de bordas de 10%, taxa = `1/attackRate`.
- **`CartWeapons/Arrow/ArrowProjectile.cs`** — projétil reto, destrói-se ao atingir `range` ou colidir com tag `"Enemy"`.

> Arma **`Magic`** existe só como dado (`EWeaponType.Magic`, `MagicLevelData`) — **sem controller implementado** (ver seção 6).

### 4.3 Inimigos / Waves — `Assets/Scripts/Enemy/`

**O que é / ideia central:** é o desafio central de cada trecho da run — ondas (waves) de inimigos que precisam ser completamente eliminadas antes de o jogador poder avançar. Cada wave define quais tipos de inimigo aparecem e em que proporção, criando variedade de dificuldade ao longo da run.

**Regras:**
- Uma wave só é considerada limpa quando **todos** os inimigos spawnados nela morrem.
- A composição da wave garante um mínimo de cada tipo configurado e distribui o restante dos inimigos proporcionalmente ao peso (`weight`) de cada tipo.
- Inimigos entram em lotes (não todos de uma vez): a cada `spawnInterval` segundos, um novo lote de até `spawnBatchSize` aparece.
- Ao limpar uma wave, o jogador ganha **+10 moedas fixas**, mas a próxima wave só começa depois que o jogador escolher uma habilidade no orbe de progressão (ver 4.4) — a wave não recomeça sozinha.
- Inimigos atacam por proximidade e cooldown fixo, e podem arremessar projéteis.

- **`Enemy.cs`** — IA via `NavMeshAgent`: persegue o player, para de mover durante animação de ataque, ataca quando `distance <= attackRange` respeitando `attackCooldown` (padrão 1s), dano padrão 15; lança `AxeProjectile` via `ThrowProjectile()`, aplicando `HordeDamageMultiplier.Active/Multiplier` se um evento de horda estiver ativo.
- **`AxeProjectile.cs`** — projétil de machado, rotação visual 720°/s, vida útil 3s, dano ao player via `LifeSystem`.
- **`EnemyAnimationController.cs`** — parâmetros de Animator (`speed`, `isWalking`, `isAttacking`).
- **`EnemySpawner.cs`** — sistema de waves (`WaveDefinition` com lista de `EnemySpawnEntry`: prefab/minCount/weight):
  - Monta o pool garantindo `minCount` de cada tipo, distribui slots restantes proporcionalmente ao `weight` (tipo de maior peso absorve o resto do arredondamento).
  - Embaralha (Fisher–Yates), spawna em lotes de `spawnBatchSize = 4` a cada `spawnInterval = 2s`, dentro de `spawnAreaSize` ao redor do spawner.
  - Espera todos morrerem antes de considerar a wave concluída; depois aguarda `OnReadyForNextWave` (disparado por `NotifyReady()`, chamado pelo `AbilityOrb` após escolha de upgrade).
  - Ao concluir a wave: **+10 moedas fixas**. Eventos estáticos: `OnWaveStarted`, `OnWaveCleared`.
- **`Assets/AttackStateBehaviour.cs`** (solto, fora de `Scripts/`) — `StateMachineBehaviour` de Animator: `OnStateEnter` seta flag de ataque, `OnStateExit` lança o projétil do `Enemy`.

### 4.4 Progressão dentro da run — `Assets/Scripts/Abilities/`, `Assets/Scripts/UI/AbilitiesUI/Abilities/`

**O que é / ideia central:** é o "level up" do roguelike — depois de limpar uma wave, o jogador escolhe 1 entre 3 cartas de recompensa (nova skill, nova arma para o vagão, ou upgrade de uma arma que já tem), progressivamente deixando o vagão mais forte ao longo da run. É a principal fonte de poder temporário do jogador (reseta a cada run).

**Regras:**
- O orbe de escolha só aparece depois que a wave é totalmente limpa, e a próxima wave fica travada até o jogador decidir.
- As 3 cartas oferecidas são sorteadas por raridade (influenciada pela sorte do jogador) entre candidatos válidos: skills ainda não exiladas, armas novas (se ainda há espaço no vagão), upgrades de armas já equipadas, ou skills específicas de uma arma já equipada.
- O jogador pode re-sortear as opções até `maxRefreshes = 2` vezes e exilar cartas indesejadas até `maxExiles = 3` vezes por seleção (itens/skills exilados não voltam a aparecer).

- **`Abilities/AbilityOrb.cs`** — orbe que ativa quando uma wave é vencida (`EnemySpawner.OnWaveCleared`); interação (E) abre a seleção de habilidades.
- **`UI/AbilitiesUI/Abilities/AbilityDrawer.cs`** — sorteia as cartas oferecidas: filtra candidatos válidos (skills não exiladas, armas não possuídas com espaço no vagão, armas possuídas com upgrade disponível, skills de arma cujo dono já esteja equipado); sorteia raridade via `RarityRoller` ponderado por `LuckPercent`, depois sorteia as cartas dentro da raridade.
- **`UI/AbilitiesUI/Abilities/AbilitySelectionUI.cs`** — tela modal (pausa o jogo), até `maxRefreshes = 2` re-sorteios e `maxExiles = 3` exílios por seleção; ao escolher, aplica o efeito e libera a próxima wave.
- **`Dto/AbilityCardData.cs`** — DTO (`IDrawable` + raridade-alvo + flag de upgrade).

Não há progressão permanente entre runs — tudo aqui reseta a cada partida (ver seção 6).

### 4.5 Loja — `Assets/Scripts/Shop/`

**O que é / ideia central:** é um local especial no mapa onde o jogador gasta as moedas ganhas na run para comprar itens permanentes (dentro da run), complementando a progressão por cartas pós-wave. O estoque muda com o tempo, incentivando o jogador a voltar à loja durante a run.

**Regras:**
- O estoque tem `slotsCount = 9` itens e se renova automaticamente a cada `refreshInterval = 180s` (3 min), ou imediatamente quando um slot fica "obsoleto" (ex.: jogador conseguiu o item por outra via, como um baú).
- Itens já possuídos pelo jogador nunca aparecem no estoque; por padrão, itens da leva anterior também não repetem (a menos que faltem candidatos suficientes).
- Quanto maior a sorte (`LuckPercent`) do jogador, maior a chance de itens raros aparecerem no estoque (mesma fórmula de raridade usada nos baús — ver 4.7).
- A compra é **tudo ou nada**: o jogador seleciona vários itens (carrinho) e só consegue confirmar se tiver moedas para pagar a soma total de tudo que selecionou — não há compra parcial do carrinho.

- **`ShopManager.cs`** — núcleo de negócio:
  - Pool de itens: `Resources.LoadAll<ItemDefinition>("Items")` em `Awake()`.
  - Estoque: `slotsCount = 9` itens (`CurrentStock`), refresh automático a cada `refreshInterval = 180s` (`Time.unscaledDeltaTime`), evento `OnStockChanged`.
  - **Sorteio (`RollNewStock`)**: exclui itens da leva anterior e itens já possuídos; se sobrarem poucos candidatos, relaxa a exclusão de "leva anterior". Sorteio ponderado sem reposição via `RarityHelper.GetWeight(rarity, luck)` — mais sorte, mais chance de raros (mesma fórmula dos baús).
  - Reage a itens adquiridos fora da loja (ex.: baú): remove do estoque + `RefillStock()` imediato.
  - **Compra tudo-ou-nada**: soma o preço de todos os itens selecionados; só executa se `Coins >= total`; debita, chama `AcquireItem` para cada item, `RefillStock()`, dispara `OnStockChanged`.
- **`ShopUI.cs`** — pausa o jogo ao abrir (`Time.timeScale = 0`), carrinho (`HashSet<ItemDefinition>`), habilita confirmação só se houver seleção e saldo suficiente para o total.
- **`ShopSlotUI.cs`** — popula slot; trava (`_locked`) se já possuído ou se o preço do item sozinho excede o saldo (checagem item a item — a validação real "tudo-ou-nada" do carrinho fica no `ShopManager`/`ShopUI`).
- **`ShopZone.cs`** — trigger (`tag == "Player"`), tecla E abre/fecha, trava movimento do player enquanto aberta; Tab alterna entre o modo Comprar (`ShopUI`) e Vender (`SellUI`) sem sair da loja.

**Venda de itens:**

**O que é / ideia central:** contraparte da compra — permite ao jogador converter itens que já possui de volta em moedas durante a run. Existe pra dar liquidez ao inventário (ex.: item que não serve mais pra build atual) sem deixar a venda tão vantajosa quanto simplesmente não ter comprado/pego o item.

**Regras:**
- O preço de venda de um item é sempre **menor** que o preço de compra (`ItemDefinition.price`): venda = compra × `(1 - sellDiscountPercent)`, com `sellDiscountPercent` configurável no Inspector do `SellManager` (padrão `0.15`, ou seja, 15% a menos).
- Só itens realmente possuídos pelo jogador (`PlayerItemHandler.AcquiredItems`) podem ser selecionados para venda.
- Igual à compra, a venda é **tudo ou nada por confirmação**: o jogador seleciona vários itens do inventário (carrinho de venda), vê o total que vai receber, e um único clique em "Vender Itens" vende todos de uma vez.
- Vender um item remove seu efeito do jogador (reverte o `StatChange` aplicado, ou remove o componente de `Ability`) e tira o item do inventário — não é possível vender o mesmo item duas vezes.
- Igual à loja de compra, cada slot de item selecionado mostra um background/frame diferenciado para indicar que está selecionado.

- **`Shop/SellManager.cs`** — núcleo de negócio, singleton simples (`Instance`, mesmo padrão do `ShopManager`): `GetSellPrice(item) = round(item.price * (1 - sellDiscountPercent))`; `TrySellMultiple` valida posse de cada item, soma o valor de venda, credita `PlayerStatsAggregator.Coins` e remove os itens via `PlayerItemHandler.RemoveItem`.
- **`Player/Items/PlayerItemHandler.cs`** — `RemoveItem(item)` reverte o efeito (`StatChange`: subtrai/desfaz o multiplicador aplicado; `Ability`: destrói o componente adicionado) e remove da lista de itens adquiridos; dispara `OnItemsChanged`.
- **`Shop/SellUI.cs`** — tela modal (pausa o jogo), lista o inventário do jogador em slots clicáveis (`SellInventorySlotUI`) e mantém um carrinho de seleção espelhado em `SellCartItemUI` (ícone + preço de venda + botão de remover do carrinho); total agregado exibido, confirmação chama `SellManager.TrySellMultiple`.
- **`Shop/SellInventorySlotUI.cs`** — slot do inventário: clique alterna seleção, `selectedBackground` reflete o estado (mesmo padrão visual do `selectedFrame` do `ShopSlotUI`).
- **`Shop/SellCartItemUI.cs`** — item já selecionado dentro do painel "Itens para vender": ícone, preço de venda, botão X para remover da seleção sem vender.
- **`Editor/SellUIBuilder.cs`** — ferramenta de editor (`RailStorm/UI/Build Sell UI` no menu do Unity) que gera a hierarquia completa da UI de venda (`CanvasItemSell`, painéis de inventário/carrinho, slots, textos, botões) com cores placeholder e já conecta as referências no `SellUI`; também localiza o `ShopZone` da cena e preenche `sellUI`/`sellManager` automaticamente (criando um `SellManager` se não existir). Idempotente — rodar de novo substitui o `CanvasItemSell` anterior. Visual gerado é só placeholder (cores sólidas); reskin com a arte do jogo fica a cargo de quem for montar a cena.

### 4.6 Eventos — Baú e Horda — `Assets/Scripts/Events/`

**O que é / ideia central:** são os "eventos especiais" que quebram o ritmo padrão de wave→progressão, dando variedade a cada trecho da run. Um **Baú** oferece um item de recompensa opcional; uma **Horda** é um desafio de combate extra (mais difícil, com inimigos mais perigosos) que recompensa bem se o jogador sobreviver. Nunca os dois ao mesmo tempo — é sempre uma escolha de risco/recompensa em cada gatilho.

> **Status atual (10/08):** a Horda está **desativada** — os assets de configuração dela foram removidos de propósito, então só o Baú está de fato jogável hoje. Ver seção 6.

**Regras (desenho original, Horda pendente de retomada):**
- Em cada gatilho (início ou fim de wave, dependendo do tipo de evento), no máximo **um** evento é sorteado: Baú (`50%` de chance por padrão), Horda (`35%`), ou nenhum evento (o restante da probabilidade).
- **Baú**: ao abrir, sorteia a raridade do item (influenciada pela sorte do jogador) e depois o item dentro daquela raridade; o jogador escolhe entre **pegar**, **exilar** (nunca mais aparece em baús) ou **pular** o item.
- **Horda**: o jogador precisa interagir com um totem para *aceitar* o desafio (não é forçado). Durante a horda, os inimigos causam **+25% de dano** (padrão) enquanto ela estiver ativa. O jogador ganha moedas por inimigo morto e um bônus maior ao final — **mas o bônus final só é pago se o jogador sobreviver** até o fim do evento.

**`EventOrchestrator.cs`** — coordenador central:
- Assina `EnemySpawner.OnWaveStarted`/`OnWaveCleared`. Cada tipo de evento tem `EventTiming` (`DuringWave` ou `AfterWave`) — padrão: Chest = `DuringWave`, Horde = `AfterWave`.
- Roleta ponderada por gatilho: `chestSpawnChance = 0.5`, `hordeSpawnChance = 0.35`, restante = chance de nenhum evento. **No máximo um evento por gatilho.**
- Tipo não elegível no gatilho atual → despawna spawners ativos e cancela spawns pendentes.
- Delay aleatório configurável (`chestSpawnDelayRange`/`hordeSpawnDelayRange`, padrão 0–0) antes do spawn efetivo.

**Sub-sistema Chest** (`Events/Chest/`):
- `ChestSpawner.cs` — escolhe marcador aleatório evitando repetir o último; `HasActiveChest` evita duplicar.
- `ChestInteractable.cs` — raio de interação `2f`, tecla E. `Open()`: sorteia raridade (`RarityRoller`, influenciado por `LuckPercent`) → sorteia item (`ChestLootRoller`) → pausa o jogo → mostra `ChestRevealEffect` com **Take / Exile / Skip**. `OnChestOpened` (evento estático) e `OnOpenedOrDespawned`.
- `ChestLootRoller.cs` — filtra pool por raridade exata e não-excluído; se vazio, faz fallback em leque (`alvo - offset`, depois `alvo + offset`, offset crescente); se ainda vazio, qualquer item não excluído; se nada, `null`.
- `ChestLootTable.cs` — ScriptableObject: `possibleItems`, `minRarity` (padrão 0), `maxRarity` (padrão -1 = usa a mais alta configurada).
- `ChestRevealEffect.cs` — UI de revelação (singleton), roda em tempo não-escalado (funciona pausado).

**Sub-sistema Horde** (`Events/Horde/`) — **desativado por ora** (10/08): os assets de configuração (`Resources/Events/HordeEventConfig.asset` e `HordeEventDefinition.asset`) foram removidos de propósito; os scripts abaixo continuam no repositório para retomada futura, mas sem dado nenhum apontando pra eles não há como o evento ser sorteado/ativado (nenhuma cena/prefab tinha um `HordeEventController` plugado no `EventOrchestrator` mesmo antes disso). Ver seção 6.
- `HordeEventConfig.cs` — `enemies` (`HordeEnemyEntry`: prefab/minCount/weight), `totalEnemies = 20`, `spawnInterval = 1s`, `spawnBatchSize = 6`, `damageMultiplier = 1.25`, `coinsPerKill = 2`, `coinsOnComplete = 50`, `eventDuration = 0` (sem limite).
- `HordeTotemSpawner.cs`/`HordeTotemInteractable.cs` — totem de aceite (raio `3f`, tecla E) → `HordeSpawner.TriggerHorde()`.
- `HordeSpawner.cs` — `BuildPool`: garante `minCount` por tipo, distribui o resto proporcional ao `weight` (maior peso absorve o arredondamento), embaralha (Fisher–Yates). Spawna em lotes; ativa `HordeDamageMultiplier.Active/Multiplier` (consumido em `Enemy.cs`) durante o evento. Término: todos mortos, ou `eventDuration` excedido se > 0. `FinishHorde()`: desativa multiplicador, concede `coinsOnComplete` **só se o player está vivo**. Eventos estáticos `OnHordeStarted`/`OnHordeEnded`.
- `HordeDamageMultiplier.cs` — ponte estática simples (`Active`, `Multiplier`) entre `HordeSpawner` e `Enemy.cs`.

### 4.7 Itens, Skills, Raridade — `Assets/Scripts/Items/`, `Assets/Scripts/Skills/`, `Assets/Scripts/Systems/Rarity/`

**O que é / ideia central:** é o vocabulário compartilhado de "recompensas" do jogo — Itens (loja/baú) e Skills (progressão pós-wave) são só dois "wrappers" diferentes em cima do mesmo conceito de efeito (mudar um stat ou dar uma habilidade nova). A Raridade é o sistema transversal que define o quão bom/raro algo é e o quão provável é aparecer, usado por todo mundo que sorteia uma recompensa (loja, baú, cartas de progressão).

**Regras:**
- Um item/skill tem exatamente um efeito: ou muda um stat do jogador (soma um valor fixo, ou multiplica percentualmente o stat atual), ou concede uma habilidade nova (comportamento em código, adicionado dinamicamente ao jogador).
- Skills só podem ser "upgradadas" para uma raridade estritamente maior que a atual — não dá pra pegar uma versão pior/igual de uma skill já adquirida.
- Existem 5 níveis de raridade (Common → Legendary); quanto mais sorte (`LuckPercent`) o jogador tem, menor o peso de raridades comuns e maior o de raras/épicas/lendárias no sorteio — a fórmula é a mesma em toda parte do jogo que sorteia por raridade.
- Um item "exilado" pelo jogador nunca mais aparece em baús, mas isso **não** o remove da loja (a loja só evita itens já possuídos, não os exilados) — comportamento assimétrico a ter em mente.

- **`Items/ItemDefinition.cs`** — ScriptableObject (`IDrawable`): `price` (só loja), `rarity`, `effectType` (`StatChange` ou `Ability`). `StatChange`: `statTarget`/`statValue`/`isMultiplier`. `Ability`: `abilityTypeName`, resolvido via `Type.GetType`, componente adicionado dinamicamente ao player.
- **`Player/Items/PlayerItemHandler.cs`** — `AcquireItem` idempotente; `ApplyStatChange` suporta `MoveSpeed`, `MaxHP`, `HP`, `Coins`, `LuckPercent` (soma ou multiplicador `%`); alvos não tratados só logam warning. `ExileItem`/`IsExiled` (exilado não reaparece em baús, mas ainda pode aparecer na loja — a loja só filtra por `HasItem`). `ResetForNewRun()` limpa tudo.
- **`Skills/SkillDefinition.cs`** — ScriptableObject genérico (`IDrawable`), níveis por raridade (`SkillLevelData`).
- **`Player/Skills/PlayerSkillHandler.cs`** — aplica/upgrade skills; upgrade só permitido se `rarityIndex > CurrentRarity`; tem lógica análoga de aplicar `Coins` como soma/multiplicador.
- **Raridade** (`Systems/Rarity/`): `RarityConfigDefinition` (singleton `RarityConfig`, `Resources/RarityConfig.asset`) — 5 níveis padrão:
  | Raridade | baseWeight | weightPerLuck |
  |---|---|---|
  | Common | 60 | -0.30 |
  | Uncommon | 25 | -0.10 |
  | Rare | 10 | +0.15 |
  | Epic | 4 | +0.15 |
  | Legendary | 1 | +0.10 |

  `RarityHelper.GetWeight(rarity, luck) = max(0, baseWeight + weightPerLuck * clamp(luck, 0, 100))`. `RarityRoller.Roll(minRi, maxRi, luck)` — sorteio ponderado num intervalo. Usado por Chest, Shop e `AbilityDrawer`.

### 4.8 Economia (moedas)

**O que é / ideia central:** as moedas são o recurso que conecta praticamente todos os sistemas da run — ganhas ao combater (waves, hordas) e gastas para progredir estrategicamente (loja, desbloqueio de trilhos). Não existe (ainda) uma segunda moeda de meta-progressão entre runs — é tudo a mesma moeda, resetada a cada partida.

**Regras:**
- O saldo nunca fica negativo (qualquer tentativa de ir abaixo de 0 é travada em 0).
- Toda run começa com um saldo inicial fixo de 50 moedas.
- Fontes de moeda são sempre eventos discretos (limpar wave, matar/completar horda, obter item de efeito `Coins`) — não há geração passiva/por tempo.
- Bônus de conclusão da horda é condicional à sobrevivência do jogador (ver 4.6) — é a única recompensa monetária do jogo com essa condição.

Não há `CurrencyManager` dedicado — `Coins` é um campo em `PlayerStatsAggregator` (`Assets/Scripts/Player/Stats/PlayerStatsAggregator.cs`), saldo inicial `50`, nunca negativo (`Mathf.Max(0, value)`).

- **Ganha**: +10 por wave limpa (`EnemySpawner`); +`coinsPerKill` (padrão 2) por kill de horda + `coinsOnComplete` (padrão 50) ao concluir horda com o player vivo (`HordeSpawner`); itens/skills `StatChange` com alvo `Coins` (soma ou multiplicador do saldo atual); venda de itens já possuídos na loja (`SellManager`, ver 4.5) — sempre por um valor menor do que o item custaria comprado.
- **Gasta**: compra na loja (`ShopManager.SpendCoins`); desbloqueio de spline (`SplineUnlockZone`).
- `EStatTarget.CoinDropRate` existe no enum mas **nenhum script o consome** ainda.

### 4.9 UI/HUD relacionada a gameplay

**O que é / ideia central:** camada de apresentação que dá feedback ao jogador sobre interações possíveis (prompts "pressione E") e mostra as telas de decisão (seleção de habilidade, inventário, stats, loja/baú). O jogo hoje é mais orientado a telas modais contextuais do que a um HUD permanente na tela.

**Regras:**
- Não há indicador permanente de vida/munição na tela normal de jogo — informação de stats só aparece dentro de telas contextuais (ex.: seleção de habilidades).
- Prompts de interação ("pressione E") são compartilhados por todos os pontos de interação do jogo (baú, horda, loja, desbloqueio de trilho) através de um único componente reutilizável.

- `UI/AbilitiesUI/Abilities/*` — seleção pós-wave (cards, drawer).
- `UI/AbilitiesUI/Inventory/*` — grade de inventário (armas/itens) via `IDrawable`.
- `UI/AbilitiesUI/Stats/*` — painel de stats (bind dinâmico em `PlayerStatsAggregator.AllStats`).
- `Totem/InteractPromptUI.cs` — prompt "pressione E" (singleton, reusado por Chest/Horde/Shop/SplineUnlockZone).
- `Totem/TotemView.cs`/`JunctionTotemsController.cs` — UI world-space dos totens de desbloqueio de trilho (partículas, shake ao negar, pulso ao desbloquear).
- `Totem/FocusDimController.cs` — Volume URP (vignette) para focar visualmente no totem durante o menu de desbloqueio.

Não há HUD clássico (vida/munição sempre visível) implementado — só painéis contextuais (stats na tela de habilidades, prompts de interação).

### 4.10 Cenas

**O que é / ideia central:** organização de cenas do projeto — hoje o jogo roda inteiro numa única cena de gameplay, sem separação entre menu, mapas diferentes ou tela de game over (consistente com a lacuna de "sem `GameManager`"/"sem múltiplos mapas" da seção 6).

- `Assets/Scenes/SampleScene.unity` — cena principal (com NavMesh bakeado).
- `Assets/Scenes/TestScene.unity` — cena de teste.
- `Assets/_Recovery/` — lixo de auto-recovery do Editor, **não são cenas de gameplay organizadas**.

## 5. Fluxo integrado (resumo)

1. `EnemySpawner` dispara `OnWaveStarted`/`OnWaveCleared` → `EventOrchestrator` decide (roleta, um evento por vez) se spawna baú ou totem de horda, conforme o `EventTiming` de cada tipo.
2. **Baú**: interação → raridade (`RarityRoller`, influenciado por sorte) → item (`ChestLootRoller`) → Take/Exile/Skip → `PlayerItemHandler.AcquireItem`.
3. **Horda**: aceite no totem → `HordeSpawner` gera pool ponderado, spawna em lotes, aplica multiplicador de dano global, recompensa por kill + bônus condicionado à sobrevivência.
4. **Loja**: `ShopZone` (E) → estoque renovado a cada 3min ou sob demanda, sorteado por raridade+sorte, compra tudo-ou-nada debitando `PlayerStatsAggregator.Coins`. Tab dentro da loja alterna para o modo Vender (`SellUI`): jogador seleciona itens do próprio inventário, vende tudo-ou-nada por `SellManager.TrySellMultiple`, recebendo `preço de compra × (1 - sellDiscountPercent)` por item.
5. Ao limpar uma wave → `AbilityOrb` → 3 cartas (skill/arma nova/upgrade) via `AbilityDrawer`/`AbilitySelectionUI` → próxima wave liberada.
6. Moedas de waves/hordas/itens alimentam loja e desbloqueio de splines.

## 6. Lacunas conhecidas (design vs. implementado)

Itens da visão do jogo (seção 1) que **ainda não existem no código**:

- **Sem `GameManager` central** — nenhuma classe orquestra estado global, transições de cena ou game over.
- **Sem save/load** — nenhum `PlayerPrefs`, `JsonUtility`, arquivo em disco, `SceneManager` ou `DontDestroyOnLoad` encontrado.
- **Sem meta-progressão persistente** — nenhuma segunda moeda entre runs; `Coins` é só por run e reseta (`PlayerItemHandler.ResetForNewRun()`).
- **Sem personagens desbloqueáveis** nem seleção de personagem.
- **Sem upgrades permanentes** entre partidas.
- **Sem boss** implementado.
- **Sem múltiplos mapas** com dificuldade progressiva.
- **Evento de Horda desativado de propósito (10/08)** — `Resources/Events/HordeEventConfig.asset` e `HordeEventDefinition.asset` foram removidos a pedido do usuário para reformular o evento mais tarde; os scripts (`Assets/Scripts/Events/Horde/*`) continuam no repo intactos, só sem dado de config apontando pra eles. Hoje só o evento de Baú roda de fato.
- **Arma `Magic`** só como dado (`EWeaponType.Magic`, `MagicLevelData`) — sem `MagicWeaponController`.
- **`EStatTarget`** já reserva alvos não consumidos por nenhum script: `EnemyDamage`, `EnemySpeed`, `EnemyHP`, `SpawnRate`, `WaveSize`, `CoinDropRate`, `XpMultiplier`.
- **`Items/Abilities/ItemExemplo.cs`, `ItemExemplo2.cs`, `ItemExemplo3.cs`** — stubs vazios/placeholder (templates de exemplo, sem lógica real).
- **Inimigos especiais** (armadilhas nos trilhos, remover o jogador do vagão) da visão do jogo — não encontrados no código atual (só `Enemy.cs` genérico com ataque corpo a corpo/projétil).
