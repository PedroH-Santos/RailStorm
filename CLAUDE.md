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
- **Regra inegociável — ScriptableObject é dado de design, nunca estado de run (25/08).** Um SO é um **asset em disco, instância única compartilhada**: escrever nele em runtime persiste a mudança entre sessões de Play no Editor e vaza entre runs no build. Nenhum campo que descreva o *progresso do jogador* (nível adquirido, posse, exílio, contadores) pode morar num SO — esse estado pertence sempre a um handler em runtime. Ver 4.12.
- **Regra inegociável — UI não é gerada por Editor script (27/08).** Nada de UI (telas, painéis, cards, ícones, texturas 9-slice) é criado por um script de `Assets/Editor/*` que desenha PNGs em código ou monta a hierarquia da cena via menu (`[MenuItem]`). Esse padrão existiu para a UI de escolha de caminho (`ChooseWayArtGenerator.cs`, `ChooseWayUIBuilder.cs`) e para as placas/brilhos de raridade (`RarityIconArtGenerator.cs`) — os três foram removidos a pedido do usuário porque viravam código órfão, sem papel na gameplay, só usado uma vez para gerar assets/objetos de cena. UI nova ou alterada é montada diretamente na cena/prefabs (hierarquia, componentes, sprites já existentes ou desenhados fora do Editor).
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
- **`Cart/PlayerCartWeaponHandler.cs`** — inventário de até `maxWeapons = 3` armas equipadas (`WeaponDefinition`); aquisição/upgrade/exílio; evento `OnWeaponsChanged`. **É o dono do progresso de armas da run** (ver 4.12): guarda o nível de cada arma, o nível de cada `WeaponSkillDefinition`, quais weapon skills estão aplicadas em qual arma, e o cache de stats efetivos. Expõe `Instance` estático (padrão do projeto) para os consumidores desacoplados.
- **`Cart/Weapons/WeaponDefinition.cs`** — ScriptableObject de arma (`IDrawable`), **dado puro e imutável**: níveis por raridade (`WeaponLevelData`), `MaxRarity`/`LevelCount`, `GetStatsForRarity`. Não guarda nível adquirido nem skills aplicadas — quem faz isso é o `PlayerCartWeaponHandler`.
- **`Cart/Weapons/WeaponLevelData.cs`** — base abstrata (dano/cadência/alcance) + `ArrowLevelData` (speed, arrowCount) e `MagicLevelData` (area, castTime). Cada subclasse expõe dois pares simétricos por `EWeaponStatTarget`: escrita (`ApplyModifier`, já existia) e **leitura** (`DisplayStats` — lista ordenada de quais targets aquele tipo de arma exibe — e `GetStatValue` — valor atual de um target). É esse par de leitura que alimenta o tooltip (4.9) e o card de recompensa (4.4) de forma data-driven; **toda arma nova (nova subclasse) precisa declarar os dois** para aparecer certo nessas telas — não editar `TooltipBuilder`/`AbilityCardUI` a cada tipo novo.
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

### 4.5 Loja — `Assets/Scripts/Store/Shop/` (compra) e `Assets/Scripts/Store/Sell/` (venda)

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
- **`ShopZone.cs`** — `SphereCollider` auto-configurado por `interactRadius` (padrão `3f`) + `interactCenter` (offset local, pra alinhar a esfera com o centro visual do prefab quando o pivô não fica no meio), compensando a escala do objeto — mesmo cálculo de `InteractableObject`/`SplineUnlockZone`. Recalcula tanto em `Awake` (runtime) quanto em `OnValidate`/`OnDrawGizmosSelected` (Editor, sem precisar dar Play), desenhando um gizmo laranja com o range real. Trigger (`tag == "Player"`), tecla E abre/fecha, trava movimento do player enquanto aberta.

**Venda de itens:**

**O que é / ideia central:** contraparte da compra — permite ao jogador converter itens que já possui de volta em moedas durante a run. Existe pra dar liquidez ao inventário (ex.: item que não serve mais pra build atual) sem deixar a venda tão vantajosa quanto simplesmente não ter comprado/pego o item. É um **local separado no mapa** (loja de venda própria, com seu próprio trigger/gatilho), não uma aba dentro da loja de compra — cada uma tem sua zona de interação independente.

**Regras:**
- O preço de venda de um item é sempre **menor** que o preço de compra (`ItemDefinition.price`): venda = compra × `(1 - sellDiscountPercent)`, com `sellDiscountPercent` configurável no Inspector do `SellManager` (padrão `0.15`, ou seja, 15% a menos).
- Só itens realmente possuídos pelo jogador (`PlayerItemHandler.AcquiredItems`) podem ser selecionados para venda.
- Igual à compra, a venda é **tudo ou nada por confirmação**: o jogador seleciona vários itens do inventário (carrinho de venda), vê o total que vai receber, e um único clique em "Vender Itens" vende todos de uma vez.
- Vender um item remove seu efeito do jogador (reverte o `StatChange` aplicado, ou remove o componente de `Ability`) e tira o item do inventário — não é possível vender o mesmo item duas vezes.
- Igual à loja de compra, cada slot de item selecionado mostra um background/frame diferenciado para indicar que está selecionado.

- **`Store/Sell/SellManager.cs`** — núcleo de negócio, singleton simples (`Instance`, mesmo padrão do `ShopManager`): `GetSellPrice(item) = round(item.price * (1 - sellDiscountPercent))`; `TrySellMultiple` valida posse de cada item, soma o valor de venda, credita `PlayerStatsAggregator.Coins` e remove os itens via `PlayerItemHandler.RemoveItem`.
- **`Player/Items/PlayerItemHandler.cs`** — `RemoveItem(item)` reverte o efeito (`StatChange`: subtrai/desfaz o multiplicador aplicado; `Ability`: destrói o componente adicionado) e remove da lista de itens adquiridos; dispara `OnItemsChanged`.
- **`Store/Sell/SellUI.cs`** — tela modal (pausa o jogo), lista o inventário do jogador (apenas `ItemDefinition`, nunca armas) em slots clicáveis (`SellInventorySlotUI`) e mantém um carrinho de seleção espelhado em `SellCartItemUI` (ícone + preço de venda); des-selecionar um item é feito clicando de novo no slot da esquerda, não há botão de remover no carrinho. Total agregado exibido, confirmação chama `SellManager.TrySellMultiple`. Slots de inventário e de carrinho **não são pré-criados na cena** — no `Awake`, `ClearContainer` apaga qualquer filho que já exista em `inventoryContainer`/`cartContainer` (ex.: slots deixados manualmente na cena só pra visualizar layout no Editor), e depois `EnsureSlots` instancia sob demanda a partir de `inventorySlotPrefab`/`cartSlotPrefab` (mesmo padrão de `InventorySection.cs`), então a UI acompanha sozinha qualquer quantidade de itens, sem sobra de objeto de teste.
- **`Store/Sell/SellInventorySlotUI.cs`** — slot do inventário: clique alterna seleção, `selectedBackground` reflete o estado (mesmo padrão visual do `selectedFrame` do `ShopSlotUI`), `rarityBackground` colorido via `RarityHelper.Color(item.rarity)` (mesma lógica do `ShopSlotUI`, mas preenchendo o fundo do slot em vez de só uma borda).
- **`Store/Sell/SellCartItemUI.cs`** — item já selecionado dentro do painel "Itens para vender": só exibição (ícone + preço de venda), sem interação própria — a seleção/deseleção é sempre feita pelo slot correspondente no inventário à esquerda.
- **`Store/Sell/SellZone.cs`** — trigger próprio (mesmo padrão do `ShopZone`, mas independente), incluindo `interactRadius`/`interactCenter`/gizmo/recalculo em Editor: tecla E abre/fecha a `SellUI`, trava movimento do player enquanto aberta. Fica em um objeto/local diferente do `ShopZone` da compra — as duas lojas não compartilham range, tecla, `Collider` ou estado entre si; cada uma tem seu próprio par Manager (regra de negócio) + Zone (presença física/interação no mapa), mesma separação de responsabilidade dos dois lados.

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
- Cada raridade carrega, além do peso, sua **identidade visual**: uma cor viva, uma placa 9-slice própria e um brilho de fundo próprio (`RarityDto.color`, `RarityDto.iconPlate` e `RarityDto.iconGlow`). Nenhuma tela decide cor, placa ou brilho por conta própria — todas passam por `RarityHelper` (ver 4.10).
- Um item "exilado" pelo jogador nunca mais aparece em baús, mas isso **não** o remove da loja (a loja só evita itens já possuídos, não os exilados) — comportamento assimétrico a ter em mente.

- **`Items/ItemDefinition.cs`** — ScriptableObject (`IDrawable`): `price` (só loja), `rarity`, `effectType` (`StatChange` ou `Ability`). `StatChange`: `statTarget`/`statValue`/`isMultiplier`. `Ability`: `abilityTypeName`, resolvido via `Type.GetType`, componente adicionado dinamicamente ao player.
- **`Player/Items/PlayerItemHandler.cs`** — `AcquireItem` idempotente; `ApplyStatChange` suporta `MoveSpeed`, `MaxHP`, `HP`, `Coins`, `LuckPercent` (soma ou multiplicador `%`); alvos não tratados só logam warning. `ExileItem`/`IsExiled` (exilado não reaparece em baús, mas ainda pode aparecer na loja — a loja só filtra por `HasItem`). `ResetForNewRun()` limpa tudo.
- **`Skills/SkillDefinition.cs`** — ScriptableObject genérico (`IDrawable`), **dado puro e imutável**: níveis por raridade (`SkillLevelData`), `MaxRarity`/`LevelCount`, `GetLevelForRarity`. Não guarda o nível adquirido (ver 4.12).
- **`Player/Skills/PlayerSkillHandler.cs`** — **dono do progresso de skills da run**: `Dictionary<SkillDefinition,int>` com o nível adquirido de cada skill. Aplica/upgrade skills; upgrade só permitido se `rarityIndex > GetRarity(skill)`; tem lógica análoga de aplicar `Coins` como soma/multiplicador. Expõe `Instance` estático.
- **Raridade** (`Systems/Rarity/`): `RarityConfigDefinition` (singleton `RarityConfig`, `Resources/RarityConfig.asset`) — 5 níveis padrão:
  | Raridade | Cor (24/08) | baseWeight | weightPerLuck |
  |---|---|---|---|
  | Common | `#93A9BA` | 60 | -0.30 |
  | Uncommon | `#35C75A` | 25 | -0.10 |
  | Rare | `#2E86F0` | 10 | +0.15 |
  | Epic | `#A94BEB` | 4 | +0.15 |
  | Legendary | `#FFB020` | 1 | +0.10 |

  As cores foram saturadas em 24/08 a pedido do usuário ("cores vivas"). Common é a exceção deliberada: fica no cinza-aço da paleta do tema, para o degrau para Uncommon ser visível. `RarityDto.iconPlate` aponta para `Assets/UI/Theme/Rarity/IconPlate*.png` e `RarityDto.iconGlow` para `IconGlow*.png`.

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
- `UI/AbilitiesUI/Inventory/*` — grade de inventário (armas/habilidades/itens) via `IDrawable`.
- `UI/Tooltip/*` — tooltip de hover dos slots do inventário (ver abaixo).
- `UI/AbilitiesUI/Stats/*` — painel de stats (bind dinâmico em `PlayerStatsAggregator.AllStats`), organizado em **grupos nomeados** (ver abaixo).
- `Totem/InteractPromptUI.cs` — prompt "pressione E" (singleton, reusado por Chest/Horde/Shop/SplineUnlockZone).
- `Totem/TotemView.cs`/`JunctionTotemsController.cs` — o totem 3D de desbloqueio de trilho (partículas, emissão, shake ao negar, pulso ao desbloquear). A placa flutuante em si é a UI de escolha de caminho — ver 4.13.
- `Totem/FocusDimController.cs` — Volume URP (vignette) para focar visualmente no totem durante o menu de desbloqueio.

**Seções do inventário (20/08).** `InventoryUI` registra **três** seções, pelo nome do GameObject filho de `EntitiesContainer`: `Weapons` ("Armas do Carro"), `Skills` ("Habilidades") e `Items` ("Itens"). A seção de habilidades existe porque a posse de skill não ficava em lugar nenhum consultável: `SkillDefinition` guarda o próprio `currentRarity`, mas não havia lista de quais skills o jogador pegou nesta run. `PlayerSkillHandler` passou a manter `AcquiredSkills` + evento `OnSkillsChanged` (mesmo padrão de `PlayerCartWeaponHandler.OnWeaponsChanged`), e ganhou `ResetForNewRun()` para limpar essa lista junto com os exílios.

**Tooltip de hover (`Assets/Scripts/UI/Tooltip/`).** Passar o mouse sobre qualquer slot do inventário (arma, habilidade ou item) abre um painel flutuante com **cabeçalho, ícone, nome, raridade, descrição e os atributos principais**.

- `TooltipTrigger` — `MonoBehaviour` no **root do prefab do slot** (`WeaponUI.prefab`), implementa `IPointerEnterHandler`/`IPointerExitHandler`. Recebe o `IDrawable` do slot via `SetSource`, chamado por `InventorySlotView.Apply`. O prefab tem um filho `HoverArea` (`Image` totalmente transparente, `raycastTarget = true`, `LayoutElement.ignoreLayout = true`) esticado sobre o slot inteiro — sem ele o hover só pegaria em cima da arte, deixando buracos entre ícone e label de nível.
- `TooltipBuilder` — traduz um `IDrawable` em `TooltipData`, e é o único lugar que sabe o que é "atributo principal" de cada tipo:
  - **Arma**: usa `GetEffectiveStats()` (portanto **já com as `WeaponSkillDefinition` aplicadas**, não os valores base) → itera `stats.DisplayStats` (lista de `EWeaponStatTarget` declarada por cada subclasse de `WeaponLevelData`, ver 4.2) e resolve label (`StatLabels.Of`) e valor formatado (`WeaponStatFormatting.Format`) dinamicamente. Nada de tipo de arma é conhecido pelo `TooltipBuilder` — é 100% data-driven, no mesmo espírito de Habilidade/Item abaixo.
  - **Habilidade**: efeito (stat alvo + valor) e `Nível atual / total`.
  - **Item**: efeito. `StatChange` vira uma linha de atributo (stat alvo + valor); `Ability` **não vira linha de atributo** — ganha o bloco "Habilidade Especial" descrito abaixo.
- **Bloco "Habilidade Especial" (`TooltipData.HasAbility`)** — itens de `effectType = Ability` mostram um bloco próprio com nome e descrição da habilidade, alimentado por dois campos novos do `ItemDefinition`: `abilityName` (opcional; se vazio a linha do nome some, já que o título do tooltip repetiria o item) e `abilityDescription`. Sem `abilityDescription` preenchido cai num texto genérico. Antes o efeito de habilidade era uma linha de atributo com valor "Habilidade especial", que não cabia na coluna de valor e quebrava em três linhas.

- **Bloco "Melhorias" (`TooltipData.Upgrades`)** — as `WeaponSkillDefinition` aplicadas a uma arma **não** entram na lista de atributos: elas têm bloco próprio, abaixo de um divisor, para o jogador ver o que já pegou de upgrade naquela arma. Cada melhoria ocupa duas linhas (`TooltipUpgradeRowUI`): nome na primeira, e na segunda o efeito (`stat alvo + delta`) à esquerda com o nível (`Nv. atual/total`) à direita, em colunas de largura reservada. Antes elas eram linhas comuns de atributo — como os nomes são longos ("Aumento de Cadência das Flechas"), o rótulo cobria a coluna de valor e o número ficava ilegível. O bloco inteiro fica desativado quando a arma não tem nenhuma melhoria, e só armas geram esse bloco.

  **Teto de tamanho:** `TooltipBuilder.MaxUpgradeLines = 5`. O que passar disso não vira linha — vira o contador `+N melhorias` no fim do bloco. É o que impede o tooltip de crescer sem limite conforme a arma acumula upgrades. **Barra de rolagem dentro do tooltip não funciona**: o painel segue o cursor e fecha no `OnPointerExit`, então o jogador não teria como levar o mouse até a barra. Por isso o limite é por conteúdo, não por scroll.
- `StatLabels` — nomes em PT-BR de `EStatTarget` e `EWeaponStatTarget`, para o tooltip não expor nome de enum.
- `WeaponStatFormatting` — formatação **numérica/unidade** de um `EWeaponStatTarget` (ex.: `Damage`→`"15"`, `AttackRate`→`"1.2/s"`, `Range`→`"15m"`), complementar ao `StatLabels` (que só resolve o nome). É valor **absoluto**, não delta — não se confunde com `FormatDelta` (usado por skill/item/upgrade de arma). `BuildSummary(WeaponLevelData)` e `BuildTransitionSummary(prev, next)` montam a string "Label valor | Label valor" (e "Label prev→next" na versão de transição) consumida pelo `AbilityCardUI` (ver 4.4) fora do tooltip — o mesmo dicionário/lógica de formatação serve as duas telas.
- `TooltipUI` — singleton com o padrão de auto-ativação do `Instance` (seção 3), instancia as linhas sob demanda a partir de um template inativo na cena (mesmo padrão de `InventorySection`/`SellUI`) e segue o cursor em `LateUpdate`, **invertendo o `pivot`** conforme o quadrante da tela para o painel nunca sair pra fora. O painel tem `CanvasGroup.blocksRaycasts = false` — sem isso ele entraria embaixo do cursor, disparando o `OnPointerExit` do slot e fazendo o tooltip piscar infinitamente. Largura fixa em `460`; o `ContentSizeFitter` é `Unconstrained` na horizontal e `PreferredSize` na vertical, então o painel cresce só pra baixo conforme a descrição e a quantidade de atributos.

> **Armadilha do TMP criado por script:** um `TextMeshProUGUI` adicionado via `AddComponent` nasce com **`enableWordWrapping = false`**, ao contrário do que acontece ao criar o objeto pelo menu do Editor. O texto simplesmente não quebra linha e vaza pra fora do painel, mesmo com o `RectTransform` na largura certa (o `VerticalLayoutGroup` estava dimensionando o rect corretamente em 412px — só o TMP ignorava). Ao montar UI por script, **sempre setar `enableWordWrapping = true` explicitamente** em qualquer texto que possa ter mais de uma linha.

> **Corolário:** rótulo e valor na mesma linha precisam de **larguras reservadas por `LayoutElement`**, não só de wrapping. Nas linhas de atributo o valor tem `preferredWidth = 100` e o rótulo `flexibleWidth = 1` com `minWidth = 0`; sem isso um rótulo longo empurra/cobre o valor mesmo com a quebra de linha ligada.

> **Atenção — formatação de valor difere por tipo:** `isMultiplier` de **item** significa *percentual* (`_stats.X * (1 + valor/100)`, em `PlayerItemHandler`), enquanto `isMultiplier` de **skill** é multiplicação literal (`_stats.X * valor`, em `PlayerSkillHandler`). Por isso `TooltipBuilder.FormatDelta` recebe uma flag e escreve `+10%` para item e `×2` para skill. Se essas semânticas forem unificadas um dia, a flag some junto.

**Estrutura do painel de Status (20/08).** Cada stat pertence a um **grupo** declarado em `PlayerStatsAggregator.RegisterDisplayStats` (campo `StatDescriptor.Group`), e o grupo é o nome do GameObject container onde as linhas são instanciadas. Grupos atuais:

| Grupo (`StatDescriptor.Group`) | Cabeçalho na tela | Stats |
|---|---|---|
| `VitalContainer` | VITAL | Vida |
| `AttributesContainer` | ATRIBUTOS | Velocidade, Sorte |
| `ResourcesContainer` | RECURSOS | Moedas |

O `EntitiesContainer` fica dentro de um **`StatsScrollView`** (`ScrollRect` vertical, `horizontal = false`, `movementType = Clamped`) com `Viewport` mascarado por `RectMask2D` e uma barra vertical fina (8px) encostada na direita em `AutoHide` — assim novos grupos/stats não estouram o painel. O `EntitiesContainer` virou o *content*: âncora no topo, `pivot (0.5, 1)` e `ContentSizeFitter` vertical em `PreferredSize`. `StatsUI` não mudou por causa disso, porque já localizava os containers de grupo por busca recursiva.

> **Efeito colateral a lembrar:** a barra come 12px da largura útil da linha. Foi o que fez "Velocidade" quebrar em "Velocidad/e" na primeira versão. As larguras da linha (`Assets/Prefabs/UI/Stats.prefab`) foram recalculadas para o novo espaço: valor com `preferredWidth = 138` (cabe "100 / 130", que pede 114) e rótulo com `flexibleWidth = 1` ficando com 180 (cabe "Velocidade", que pede 166). Ambos com `enableWordWrapping = false` e `overflowMode = Ellipsis`, para que um rótulo grande demais corte em vez de empurrar o valor.

Na cena, cada grupo é um bloco `Group*` contendo um `Header` (faixa navy com régua bronze embaixo, o título em Fredoka e um par de ornamentos `WingLeft`/`WingRight` ladeando o título) **mais** o container das linhas. O ornamento é o sprite `Assets/UI/Theme/Ornaments/TitleWing.png` — uma cunha hachurada gerada proceduralmente, tintada com `panelBorder`; o lado direito é o **mesmo sprite espelhado** por `localScale.x = -1`, não um segundo arquivo.

> **Regra ao espelhar com `localScale`:** o espelhamento é feito **em torno do `pivot`**, não do centro do rect. Com `pivot.x = 1` (ancorado na borda direita), `localScale.x = -1` joga o sprite inteiro pra fora do painel. Por isso os dois `Wing*` usam `pivot = (0.5, 0.5)` e são posicionados por `anchoredPosition.x = ±(inset + largura/2)` — assim o espelhamento acontece em torno do próprio centro e as bordas ocupadas não mudam. Como o `Header` é irmão do container e não pode ser apagado a cada bind, `StatsUI.Bind` **não** varre `entitiesContainer` filho a filho: ele localiza cada container por busca recursiva pelo nome do grupo (`FindDeep`) e limpa apenas os filhos desse container. Isso é o que permite ter decoração fixa dentro do painel de status sem ela ser destruída no primeiro bind.

A linha de stat (`Assets/Prefabs/UI/Stats.prefab`) virou uma placa própria: fundo `#16324F`, barra de acento bronze à esquerda (`Accent`, `ignoreLayout = true`), altura fixa 50, rótulo à esquerda e valor à direita, ambos em Nunito (`ApplyStatLabel`/`ApplyStatValue`) — antes era só um par de textos soltos, sem delimitação, o que fazia os stats parecerem dispersos no painel.

> **Armadilha de layout (custou uma rodada de debug):** um `HorizontalLayoutGroup`/`VerticalLayoutGroup` com `childForceExpandHeight = true` reporta **flexibleHeight = 1 para si mesmo**, e isso sobe pela árvore inteira de layouts. Com a flag ligada no prefab da linha, cada linha "puxava" a altura sobrando do painel e os grupos esticavam para ~3× o tamanho, mesmo com `LayoutElement.preferredHeight = 50` e com `childForceExpandHeight = false` em todos os containers acima. Ao montar linha/slot de altura fixa, deixe `childForceExpandHeight = false` no próprio item.

Não há HUD clássico (vida/munição sempre visível) implementado — só painéis contextuais (stats na tela de habilidades, prompts de interação).

### 4.10 Identidade visual da UI — `Assets/Scripts/Systems/UITheme/`

**O que é / ideia central:** define uma paleta de cores e um par de fontes únicos pra todas as telas de UI do jogo (loja, venda, seleção de habilidade, baú, stats, inventário), pra elas pararem de parecer telas soltas com skins diferentes e passarem a parecer parte do mesmo jogo. Referência visual: UI de **Megabonk** e **Hades** (painel escuro com borda ornamentada, título em destaque).

**Regras:**
- Existe **um único** conjunto de cores/fontes "de tema" (`UIThemeConfig`), carregado como singleton via `Resources.Load` (mesmo padrão do `RarityConfig`) — nenhuma tela deve hardcodar cor/fonte de título/corpo no Inspector por conta própria.
- **Paleta oficial (24/08)** — 5 cores base, adotadas a partir de uma referência escolhida pelo usuário. Toda cor da UI é uma dessas 5 ou uma derivada declarada abaixo; nada de tom novo inventado por tela:

  | Nome | Hex | Papel |
  |---|---|---|
  | Steel | `#5D839B` | azul-acinzentado médio — ação neutra (botão `Pular`), texto secundário |
  | Navy Deep | `#0A1E33` | fundo padrão de painel e card — escurecido em 24/08 a pedido do usuário; o `#0F2E4C` da referência ficou claro demais no jogo |
  | Brown Deep | `#663300` | campo escuro da moldura, sombras do acento |
  | Copper | `#BC621B` | acento vivo — réguas, bullets, ornamentos, botão `Atualizar` |
  | Cream | `#FCF8E6` | títulos e valores em destaque |

  Derivadas (calculadas a partir das 5, não são cores novas de fato):

  | Nome | Hex | Origem | Papel |
  |---|---|---|---|
  | Navy Raised | `#112942` | Navy Deep clareado | faixas de título, cabeçalho de grupo de status |
  | Navy Recess | `#050F1C` | Navy Deep escurecido | encaixe afundado do slot de inventário |
  | Steel Light | `#ACBDC0` | Steel ↔ Cream a 50% | texto de corpo/descrição |
  | Vermelho de exílio | `#A8392A` | — | única cor fora da paleta, reservada à ação destrutiva (`Exilar`) |
- **Fontes (24/08)** — três papéis fixos, definidos pelo usuário. Nenhuma tela escolhe fonte por conta própria: pede o papel ao `UIThemeConfig`.

  | Fonte | Papel | Onde aparece |
  |---|---|---|
  | **Fredoka** | fonte **oficial** do jogo — o padrão. Na dúvida, é ela | descrições, rótulos de seção, raridade, nível, contadores, cabeçalhos secundários |
  | **Nunito** | estatísticas de itens e armas | linhas do painel de Status, linhas de atributo do tooltip, efeito/nível das melhorias |
  | **Lilita One** | títulos importantes | título de painel, nome da carta, rótulo de botão, cabeçalho e título do tooltip |

  Os `TMP_FontAsset` ficam em `Assets/Fonts/Generated/`: `Fredoka-VariableFont_wdth,wght SDF`, `Nunito SDF` (gerado de `Nunito/static/Nunito-SemiBold.ttf` — o `Nunito-Italic-… SDF` que já existia era da variante itálica e não serve para números), `LilitaOne-Regular SDF`. Todos em `AtlasPopulationMode.Dynamic`. As fontes anteriores (MedievalSharp/Cinzel) saíram de uso; os SDF continuam no repo, mas os `.ttf` de origem já não existem mais, então elas não conseguem mais crescer o atlas — não voltar a usá-las.
- Cantos dos painéis e cards: pouco arredondados (não retos, não muito curvos) — controlado pelos sprites 9-slice usados nos frames, não por código.
- A cor de raridade (`RarityConfig`/`RarityHelper`) continua sendo a exceção intencional: ela colore fundo/borda de card conforme a raridade do item, por cima da paleta base do tema.

- **`UIThemeConfig.cs`** — ScriptableObject singleton (`Resources/UIThemeConfig.asset`), expõe a paleta inteira como campos nomeados por **papel**, não por cor: `panelBackground` (Navy Deep `#0A1E33`), `panelSurface` (Navy Raised `#112942`), `panelRecess` (Navy Recess `#050F1C`), `panelBorder` (Copper), `panelBorderDark` (Brown Deep), `textTitle` (Cream), `textBody` (Steel Light), `textMuted` (Steel), mais os três `TMP_FontAsset` (`titleFont` = Lilita One, `bodyFont` = Fredoka, `statsFont` = Nunito).

  Os métodos `Apply*` são a **única** porta de entrada — cada um casa uma fonte com uma cor, e é escolhendo o método que a tela declara o papel do texto:

  | Método | Fonte | Cor |
  |---|---|---|
  | `ApplyTitle` | Lilita One | `textTitle` |
  | `ApplyBody` | Fredoka | `textBody` |
  | `ApplyBodyHighlight` | Fredoka | `textTitle` |
  | `ApplyMuted` | Fredoka | `textMuted` |
  | `ApplyStatLabel` | Nunito | `textBody` |
  | `ApplyStatValue` | Nunito | `textTitle` |

  Consumidores de texto: `AbilityCardUI` (`ApplyTitle` no nome, `ApplyBody` no resto), `StatRowUI` e `TooltipStatRowUI` (`ApplyStatLabel`/`ApplyStatValue`), `TooltipUpgradeRowUI` (`ApplyBodyHighlight` no nome da melhoria, `ApplyStatLabel` no efeito e no nível), `TooltipUI`, `InventorySlotView` (`ApplyBodyHighlight` no nível do slot).

  **Cores que não são texto (25/08).** Até então só fonte+cor de texto passavam pelo tema; sombra, contorno, cor de botão e bandeja de slot ficavam cravados no Inspector de cada objeto. Hoje também são campos do `UIThemeConfig`:

  | Campo | Valor | Papel |
  |---|---|---|
  | `slotTray` | `#0C2238` | bandeja do slot de inventário |
  | `actionPrimary` | `#BC621B` | botão de ação principal (`Atualizar`) |
  | `actionNeutral` | `#5D839B` | botão de saída sem consequência (`Pular`) |
  | `actionDestructive` | `#A8392A` | botão destrutivo (`Exilar`) |
  | `dropShadow` | preto 55% | sombra projetada de peças (ex.: `SlotShadow`) |
  | `outlineDark` | `#0A0805` a 85% | contorno de ícone branco e de número sobre cor |
  | `textShadow` | `#150A03` a 80% | sombra de entalhe em títulos, níveis e rótulos de botão |
  | `screenDim` | `#04101F` a 69% | escurecimento do jogo atrás de tela modal |
  | `screenDimExile` | `#990000` a 85% | escurecimento durante o modo de exílio |

  E ganharam métodos `Apply*` no mesmo padrão "o método declara o papel": `ApplyIconOutline(Shadow)`, `ApplyTextShadow(Shadow)`, `ApplyPrimaryAction(Button)`, `ApplyNeutralAction(Button)`, `ApplyDestructiveAction(Button)`. Quem aplica: `AbilitySelectionUI.ApplyTheme` (os três botões, os rótulos, os contadores e as duas cores de dim) e `InventorySlotView.ApplyTheme` (bandeja, sombra e contornos do slot).

  > **Cor no Inspector agora é só preview.** Toda cor da tela de habilidade é escrita em runtime a partir do asset, então o valor gravado na cena/prefab serve apenas para o Editor não mostrar objeto branco. Isso vale tanto para as cores de tema quanto para as de raridade (`AbilityCardUI.Setup`, `InventorySlotView.Apply`). Ao mexer numa cor, mexa no `Resources/UIThemeConfig.asset` — mudar no objeto não tem efeito em jogo.

  > **Por que isso importa (custou uma correção).** A bandeja do slot nasceu como valor literal no prefab e ficou órfã do tema: não havia nada ligando `#0C2238` à paleta, e uma mudança futura de paleta a deixaria para trás. Foi o que motivou a varredura — vale a mesma regra para qualquer cor nova.
- **`AbilityCardUI.cs`** — primeiro consumidor via script: aplica `ApplyTitle`/`ApplyBody` nos textos do card (nome, descrição, nível) e deriva de `RarityHelper` tudo que é colorido por raridade (`cardBackground`, `cardBorder`, `cardFill`, `rarityText`) — ver "Raridade como cor do card" abaixo.
- **Sprites 9-slice próprios** (`Assets/UI/Theme/`, gerados via editor script, SDF de rounded-box, sem dependência de asset generation por IA): `PanelFrame9Slice.png` (fundo `panelBackground` + borda `panelBorder` já cravada na arte, usado em `SkillPanel`/`InventoryPanel`/`StatsPanel`), `CardFrame9Slice.png` (mesma borda, fundo `cardFill` mais claro, usado nos cards), `BorderOnly9Slice.png` (centro transparente, só o anel da borda em branco — feito pra ser tintado por `Image.color`, usado como camada extra sobre o preenchimento sólido dos botões, já que cada botão tem uma cor de função diferente).
- **Moldura ornamentada dos painéis (20/08).** Os 3 painéis da tela de habilidade (`InventoryPanel`, `SkillPanel`, `StatsPanel`) usam agora **`Assets/UI/Theme/OrnateFrame9Slice.png`** no lugar do `WoodPanelFrame9Slice`: moldura de couro escuro com bisel metálico na aresta externa, um sulco corrido no meio da faixa, um bead na aresta interna e **chapas de canto com 3 rebites cada**. O sprite é 256×256 com `spriteBorder = 56` e é aplicado com `Image.pixelsPerUnitMultiplier = 2` (a borda desenha ~28px em tela).

  **Recolorida para a paleta nova (24/08).** `OrnateFrame9Slice.png` e `TitleBand9Slice.png` não foram redesenhadas: passaram por um remapeamento pixel a pixel em HSV, que mantém o desenho e troca só a família de cor. Pixels com `saturation < 0.14` (contornos neutros) ficam intactos; matiz quente vai para uma rampa `#1B0C00 → #663300 → #BC621B → #E69549` indexada pelo *value* original, e matiz azul vai para `#04101F → #0F2E4C → #2E6699`. O campo de couro fica no trecho escuro da rampa e só o filete/chapas de canto chegam no Copper — se a rampa for mais clara no meio, a moldura inteira vira laranja e passa a competir com o conteúdo. O `WoodCardFrame9Slice` continua nos **cards** — a diferença de material entre painel (metal) e card (madeira) é intencional, dá hierarquia. As runas entalhadas e a moldura de madeira dos painéis saíram junto com a troca; o `WoodPanelFrame9Slice.png` segue no repo, sem uso.

  Como todo detalhe reconhecível fica dentro da região de borda (chapas e rebites em `u,v <= 44`, dentro dos 56 do `spriteBorder`), a moldura respeita a regra do 9-slice: só o sulco e o bisel — que são linhas paralelas à borda — atravessam as faixas esticadas.

  **Recuo do conteúdo:** com a borda em ~28px, o `padding` dos 3 painéis subiu de 22 para **34**; os containers internos (`CardsContainer`, `ButtonsContainer`) baixaram de 38 para 26 para manter o espaçamento efetivo de antes.

- **Botões da tela de habilidade.** `Exilar`/`Pular`/`Atualizar` têm tamanho fixo por `LayoutElement` (230×76, 250×76 no `Atualizar`) e texto Lilita One em 34 — antes herdavam a altura do container e ficavam com ~34px de altura e texto 24, ilegíveis. O `ButtonsContainer` reserva 114px de altura.

  **Placa de botão físico (24/08).** O preenchimento usa **`Assets/UI/Theme/ButtonPlate9Slice.png`** (96×96, `spriteBorder = 26`, `pixelsPerUnitMultiplier = 1.5`), redesenhado para o botão parecer uma **peça apertável** e não um retângulo colorido — que era a queixa do usuário. De fora para dentro, de cima para baixo:

  | Faixa | Valor de cinza | Papel |
  |---|---|---|
  | contorno externo (~3px) | `0.10` | recorta o botão contra o painel |
  | brilho interno no topo (~4px) | face × `1.12` | a luz batendo na quina de cima |
  | face | `1.00` no topo → `0.92` | a superfície que o dedo aperta |
  | sulco (2px) | `0.15` | separa a face do lábio |
  | lábio inferior (~16px em tela) | `0.30`–`0.40` | a lateral da peça, vista de cima — é isso que dá altura ao botão |

  A face é quase **plana de propósito**: só o topo (dentro da região de borda do 9-slice) carrega o degradê, e a faixa central fica constante. Se a face tivesse degradê contínuo, o esticamento vertical do 9-slice deformaria o brilho conforme a altura do botão. O lábio e o sulco vivem inteiros dentro da **borda inferior** (`26px` nativos), então também não esticam.

  Cores de função por cima da placa: `Exilar` `#A8392A`, `Pular` `#5D839B` (Steel), `Atualizar` `#BC621B` (Copper). Como o lábio está em `~0.35`, uma única cor produz face viva + lateral escura automaticamente — não há segundo objeto de "sombra" embaixo do botão.

  **O texto é centralizado na face, não no botão.** `ButtonText` usa `offsetMin.y = 16` / `offsetMax.y = -4` para descontar o lábio; sem isso o rótulo fica visualmente baixo, encostado na base. O `Shadow` do texto está em `#150A03` (alpha 0.8) deslocado `(0, -2.5)`.

  O contador de usos restantes fica no `CountBadge`, **centralizado horizontalmente logo acima do botão**. Ele **não tem `Image` de fundo**: o número se destaca por `Outline` escuro + `Shadow`, sem plaquinha atrás. As referências `exileCountText`/`refreshCountText` do `AbilitySelectionUI` apontam pro `CountText` dentro dele.

- **Hierarquia de texto do card (25/08).** Os quatro textos do card estavam todos entre 21 e 30 e o card lia como um bloco cinza uniforme — a queixa foi "sem evidência" em comparação com o Megabonk. Os tamanhos agora se separam por importância:

  | Texto | Antes → Agora | Fonte | Cor |
  |---|---|---|---|
  | `Title` (nome) | 30 → **38** (autosize 24–38) | Lilita One, Bold | `textTitle` |
  | `Level` ("Nível 5" / "NOVO") | 21 → **34**, Bold, alinhado à direita | Lilita One | **cor da raridade** |
  | `Rarity` ("Lendária") | 24 → **28**, Bold | Fredoka | cor da raridade |
  | `Description` | 23 → **26** (autosize 20–26) | Fredoka | `textBody` |

  `Title`, `Level` e `Rarity` ganharam `Shadow` `#150A03` (alpha 0.8) deslocado ~2px, o mesmo tratamento dos títulos de painel e dos rótulos de botão.

  > **O nível deixou de ser texto de corpo.** `AbilityCardUI.Setup` chamava `theme.ApplyBody(levelText)`, o que o deixava em Fredoka cinza — igual à descrição, que é a informação menos importante do card. Agora `Setup` aplica `theme.titleFont` e pinta o texto com a **cor da raridade** (mesmo tratamento do `rarityText`), então o nível vira o segundo ponto de leitura do card, como o "LVL 5" amarelo do Megabonk. Por isso ele não usa `ApplyTitle`: o método também cravaria `textTitle` por cima da cor da raridade.

  > **Descrição de skill de arma não era preenchida (bug, 25/08).** A cadeia de `if/else` do `Setup` cobria `SkillDefinition` e `WeaponDefinition`, mas **não** `WeaponSkillDefinition` — cartas de melhoria de arma ficavam com o texto do card anterior (ou com o placeholder da cena, "DESCRICAO DO ITEM"). Agora `descriptionText` é zerado antes da cadeia, e há um ramo para `WeaponSkillDefinition` que usa `description` do asset ou, se vazio, monta a linha a partir de `StatLabels.Of(statTarget)` mais o valor do nível.

  > **"NOVO" no lugar do nível (25/08).** Quando a carta oferece algo que o jogador **ainda não tem**, o campo de nível mostra `NOVO` em vez de `Nível 1` — dizer "Nível 1" para um item inédito não informa nada, enquanto "NOVO" é a informação que de fato muda a decisão. Quem decide é o `AbilityDrawer`, não o card: ele já consulta posse para calcular a raridade mínima (`skillHandler.HasSkill`, `weaponHandler.HasWeapon`, `weaponHandler.HasWeaponSkill`), então grava o resultado em `AbilityCardData.isNew` e o `AbilityCardUI.Setup` só lê a flag. **Não** dá para inferir isso de `isUpgrade`: essa flag existe só para distinguir upgrade de arma equipada, e uma skill nova também chega com `isUpgrade = false`.

  > **Autosize só funciona se a linha não puder crescer (25/08 — bug encontrado em jogo).** Ligar `enableAutoSizing` no `Title` não bastou: `InfoTexts` tem `childControlHeight`, então a altura da `TitleRow` vinha do tamanho preferido dos filhos. Com o nome quebrando em 3 linhas, o TMP reportava uma altura preferida maior, a linha crescia junto e o texto **nunca estourava o próprio rect** — o autosize entendia que cabia e mantinha o corpo cheio. Quem estourava era o card, que tem 200px fixos. O conserto é travar a altura: `TitleRow` com `preferredHeight = 78` e `flexibleHeight = 0`, `Description` com `preferredHeight = 82`, `spacing = 8` no `InfoTexts` e `childForceExpandHeight = false` (78 + 8 + 82 = 168, a altura útil do card). Só então o autosize passa a agir — hoje "Aumento de Cadência das Flechas" cai para ~34pt em 2 linhas, enquanto "Skill Life" continua nos 38 cheios.

  > **`childForceExpandWidth` na `TitleRow` também precisou sair.** Com a flag ligada, a sobra horizontal é dividida em partes iguais entre `Title` e `Level`, **ignorando o `flexibleWidth = 0`** do `Level` — o rótulo de nível ficava com ~200px e espremia o nome. Desligada, o `Level` fica no `preferredWidth = 150` e todo o resto vai para o nome (~445px).

- **Proporção da placa de ícone no card (25/08).** A placa era **221×108** (paisagem 2:1) e dominava o card. Passou para **112×128** (retrato), na proporção do Megabonk, onde o ícone é uma peça vertical estreita à esquerda da linha. O que mudou na cena:

  | Objeto | Ajuste |
  |---|---|
  | `ImageContainer` | `LayoutElement.preferredWidth = 168`, `flexibleWidth = 0` — a coluna do ícone deixou de disputar largura com os textos |
  | `ImageContainer` (VLG) | `childForceExpandWidth = false`, `childAlignment = UpperCenter`, `spacing = 8` |
  | `Rarity` | `preferredHeight` 50 → **32**, fonte 28 → **26** — é o que libera altura para a placa |
  | `Background` (placa) | `preferredWidth = 112`, `preferredHeight = 128`, ambos `flexible = 0` |
  | `Icon` | margem uniforme de 14px dentro da placa (`offsetMin/Max`), `preserveAspect = true` |

  > **A altura do container é o orçamento.** `ImageContainer` tem 168px (card 200 − padding 32). Rarity 32 + spacing 8 + placa 128 = 168 — encaixa exato. Para deixar a placa mais alta é preciso tirar de algum dos outros dois, não do card.

  > **Por que `childForceExpandWidth = false`.** Com a flag ligada (como estava), o `VerticalLayoutGroup` estica todo filho até a largura do container, então o `preferredWidth = 112` da placa era ignorado e ela voltava a ocupar a coluna inteira. Desligando a flag, a placa fica no tamanho pedido e o rótulo de raridade — que precisa de mais largura que ela para caber "Lendária" — continua livre para usar os 168 do container.

- **Seleção do card animada.** `SelectionBracketsAnimator` (no mesmo GameObject `SelectionBrackets`) faz **cada cantoneira avançar na direção do centro do card e voltar**, em ciclo (`amplitude = 8px`, `cycleDuration = 1.1s`). A direção de cada uma é derivada do próprio `anchorMin`, não configurada à mão: `x < 0.5 → +1`, senão `-1` (idem em y), o que dá `(1,-1)` na superior-esquerda, `(-1,-1)` na superior-direita e assim por diante. A fase usa `(1 - cos)/2`, que vai de 0 a 1 e volta a 0 — as cantoneiras **só entram e retornam**, nunca passam para fora do repouso.

  Usa `Time.unscaledDeltaTime` porque a tela roda com o jogo pausado (`Time.timeScale = 0`), e guarda/restaura a posição de repouso em `OnEnable`/`OnDisable` — sem isso o card acumularia deslocamento a cada vez que fosse reciclado.

- **Hover dos botões.** O `ColorTint` padrão do `Button` é quase imperceptível (o `highlightedColor` default fica em ~0.96). Os três botões usam `normalColor = 1.0`, `highlightedColor = 1.16`, `pressedColor = 0.72`, `disabledColor = 0.45` (alpha 0.6) e `fadeDuration = 0.06`. O repouso ficou em `1.0` (e não rebaixado, como na versão anterior) porque a `ButtonPlate9Slice` já entrega a cor de função cheia; o hover sobe acima de 1 — valores `> 1` são válidos em código e clareiam de verdade, coisa que o Inspector não deixa digitar.

> **Ícones brancos pedem contorno escuro.** Os `UI_Icon_*` são glifos brancos. O `Outline` creme (`textTitle`) que existia no slot de inventário criava um halo amarelado em volta do desenho; hoje o contorno é quase preto (`#0A0705`, alpha 0.85) tanto no slot quanto no card. E todo ícone precisa de `preserveAspect = true`: o `Icon` do card fica num rect 200×108 e, sem a flag, o sprite quadrado era esticado na horizontal.

- **Raridade como cor do card (referência: Megabonk).** As camadas do card recebem a cor de raridade em intensidades diferentes, todas derivadas em runtime por `AbilityCardUI.Setup` — **não há nada configurado por raridade no Inspector**:
  - `cardBackground` (placa do ícone) → cor cheia da raridade, **e também troca de sprite** para a placa da raridade (ver "Placa de ícone por raridade" abaixo).
  - `iconGlow` (camada `Pattern`, entre a placa e o ícone) → troca de sprite para o brilho da raridade e recebe `RarityHelper.GlowColor(ri)` (ver "Brilho de fundo por raridade" abaixo).
  - `cardBorder` (novo filho `CardBorder`) → cor cheia da raridade. É o anel que identifica o card à distância.
  - `cardFill` (corpo do card) → `Color.Lerp(panelBackground, Shade(cor, cardFillDarkness), cardFillRarityBlend)`, com `cardFillDarkness = 0.3` e `cardFillRarityBlend = 0.7` no Inspector.
  - `rarityText` → cor cheia da raridade (o texto "Comum"/"Rara"/"Lendária"). Por isso `Setup` aplica só a **fonte** do tema nesse texto, nunca `ApplyBody`, que sobrescreveria a cor.

  > **Por que escurecer antes de misturar, e não misturar direto com o navy.** A versão anterior fazia `Lerp(panelBackground, corDaRaridade, blend)`. Misturar navy com uma raridade **quente** (o amarelo lendário) anda pelo meio da roda de cores e produz um verde-oliva sujo, sem relação com a raridade; a raridade azul, ao contrário, ficava clara demais e brigava com o painel. Escurecer primeiro (`Shade`) preserva o matiz e só derruba o brilho, então o corpo do card vira "a mesma cor, no escuro" — que é exatamente a leitura do Megabonk: card escuro, borda viva.

- **Placa de ícone por raridade (25/08).** `Assets/UI/Theme/Rarity/IconPlate{Common,Uncommon,Rare,Epic,Legendary}.png` — 64×64, `spriteBorder = 26`, aplicadas com `pixelsPerUnitMultiplier = 1` (assim o detalhe desenha em tamanho nativo). Todas em tons de cinza, tintadas em runtime com a cor da raridade. Estrutura, de fora para dentro: contorno `0.10` (~2px) → aro `1.0` (~3.6px, é ele que vira a cor viva) → **corpo escuro** em degradê vertical `0.30` no topo → `0.15` na base.

  > **Por que o corpo é escuro (25/08).** Até 24/08 o corpo ficava em `0.88`→`0.58`: a placa inteira era a cor da raridade em brilho cheio, e o ícone branco por cima não tinha contraste — era a queixa de "fica difícil ver o que é o ícone". Hoje só o aro carrega a cor viva (mesma leitura do Megabonk: placa escura, borda viva). É também o que permite o brilho descrito abaixo aparecer: sobre um corpo claro, qualquer halo some.

  O detalhe que diferencia cada raridade agora é **claro** (`0.58`) contra o corpo escuro — o inverso da versão anterior, em que era um entalhe escuro sobre corpo claro:

  | Raridade | Detalhe embutido |
  |---|---|
  | Comum | nenhum — placa lisa |
  | Incomum | linha fina recuada, acompanhando a borda |
  | Rara | + triângulos sólidos nos 4 cantos |
  | Épica | + cantoneiras em "L" nos 4 cantos |
  | Lendária | + cantoneiras mais grossas com uma gema (losango, `0.94`) cravada em cada canto |

- **Brilho de fundo por raridade (25/08).** `Assets/UI/Theme/Rarity/IconGlow{Common,Uncommon,Rare,Epic,Legendary}.png` — 256×256, **sem** `spriteBorder` (`Image.Type.Simple`), brancas com o desenho inteiro no canal alpha. Vivem na camada `Pattern` (filha da placa, `SetSiblingIndex(0)`, portanto entre a placa e o ícone) e **substituíram o risco diagonal** que ficava ali antes. A ideia é a das HUDs mobile de referência: uma luz atrás do ícone que o coloca em evidência, em vez de uma textura que compete com ele.

  | Raridade | Brilho |
  |---|---|
  | Comum | só o halo radial suave, sem enfeite |
  | Incomum | halo + poeira de círculos **pequenos** |
  | Rara | halo + círculos **médios**, parte deles em anel |
  | Épica | halo + círculos **grandes** + aro externo + 4 faíscas |
  | Lendária | halo + raios saindo do centro + 6 faíscas + aro externo |

  A intensidade do halo escala com a raridade (`0.55` no Comum → `0.88` no Lendário), e o centro leva uma queda de 16% para o ícone não se perder justamente no ponto mais claro.

  > **`preserveAspect = true` é obrigatório nessa camada.** O `Pattern` é esticado no rect da placa, que é 221×108 no card e 88×88 no slot. Sem a flag o halo circular vira elipse no card. Com ela, o sprite quadrado desenha `108×108` centralizado no card e `88×88` no slot — **um único sprite por raridade serve as duas telas**. Foi isso que aposentou o par `IconPlateStripes`/`IconPlateStripesWide`, que só existia porque um risco a 45° precisa de um sprite com o mesmo aspecto do rect. Os dois PNGs continuam no repo, sem uso.

  > **Por que o brilho é uma camada separada, e não parte da placa.** A placa é `Image.Type.Sliced`: a faixa central é **esticada**, então um halo desenhado nela seria cortado nas emendas do 9-slice. O `Pattern` é `Type.Simple` e nunca tem emenda. O alpha do brilho também cai a zero antes da borda do sprite, para o retângulo do `Pattern` não vazar pelos cantos arredondados da placa.

  **Ligação com o dado, não com a cena:** placa e brilho são campos do `RarityDto` (`iconPlate`, `iconGlow`), configurados no `Resources/RarityConfig.asset` e lidos por `RarityHelper.IconPlate(int)` / `RarityHelper.IconGlow(int)`. A cor do brilho **não** é a cor crua da raridade: `RarityHelper.GlowColor(int)` mistura 30% de branco, senão o halo lê como "mais da mesma cor" em vez de luz. Quem consome: `AbilityCardUI.Setup` (campo `iconGlow`), `InventorySlotView.Apply` (acha o `Pattern` por `FindDeep`, mesmo padrão do resto do slot) e `TooltipUI` (campo `iconGlow`, raridade via `TooltipData.RarityIndex`). Nenhum desses scripts conhece nome de arquivo — para trocar a arte de uma raridade basta apontar outro sprite no asset.

  **Os 10 PNGs (5 placas + 5 brilhos) são assets estáticos em `Assets/UI/Theme/Rarity/`**, referenciados pelo `RarityConfig.asset`. O script que os havia gerado (`Assets/Editor/RarityIconArtGenerator.cs`) foi removido (27/08) — o projeto não usa mais Editor scripts para gerar arte/UI por código (ver seção 3); qualquer ajuste visual nessas placas/brilhos daqui em diante é feito editando os PNGs diretamente, não regenerando via ferramenta.

  > **Regra do 9-slice vale aqui também:** todo detalhe reconhecível (triângulos, cantoneiras, losangos) fica dentro dos `26px` da região de canto. O único elemento que atravessa as faixas esticadas é a linha do Incomum — e ela é paralela à borda, então esticar não a deforma. Foi por isso que a `spriteBorder` subiu de `20` para `26`: com `20`, a ponta do braço da cantoneira caía na faixa central e esticava junto com a placa.

- **Anatomia das placas (`CardPlate9Slice`, `CardBorder9Slice`).** Ambas 96×96 com `spriteBorder = 26` e `pixelsPerUnitMultiplier = 1.5`, geradas pela mesma SDF de rounded-box (`margin = 2`, `radius = 17`) — é essa origem comum que garante que o anel encaixe exatamente na quina da placa. `CardPlate9Slice` é o corpo (contorno `0.26` + degradê `0.78`→`1.0`); `CardBorder9Slice` é só o anel de `6px` em branco puro, com o miolo transparente.

  Todas as placas do tema são **em tons de cinza para serem tintadas** por `Image.color`: como o aro fica em `1.0` e o corpo abaixo de `1.0`, uma única cor produz borda clara + corpo escuro automaticamente. É isso que faz cada peça parecer uma peça, e não um retângulo de cor colado.

  > **Lição da primeira tentativa:** a versão anterior (`CardFill9Slice`/`ButtonFrame9Slice`, ainda no repo sem uso) usava gradientes suaves, ruído e bordas de baixo contraste — e o resultado foi descrito pelo usuário como "com cara de IA". O que dá leitura de UI de jogo é o oposto: **contorno nítido de alto contraste e bisel duro**, com pouco ou nenhum ruído.

  > **Cuidado ao combinar com `Button.colors`:** o tint do `Button` **multiplica** a cor da `Image`. Com o corpo da placa em ~0.5, um `normalColor` de 0.78 derrubava o botão para ~0.39 da cor de função e ele ficava quase preto. Por isso a face da placa de **botão** fica entre `0.92` e `1.0` e o tint parte de `normalColor = 1.0` (ver "Hover dos botões").

  > **Cuidado com o default do Editor:** como a cor real só é aplicada em `Setup`, a cor gravada na cena é apenas preview. Ela está setada na raridade Common — se ficar em branco, o card aparece lavado no primeiro frame antes do `Setup` rodar.

- **A borda do card virou uma camada separada (24/08).** A `Image` do root do card continua sendo a placa (`CardPlate9Slice`, tintada por `AbilityCardUI.cardFill`), mas o anel de raridade saiu do sprite da placa e virou o filho **`CardBorder`** — `CardBorder9Slice`, `LayoutElement.ignoreLayout = true`, esticado no card, `raycastTarget = false`, `SetSiblingIndex(0)`. Isso é o que permite corpo escuro e borda viva ao mesmo tempo: com o anel embutido na placa, os dois compartilhariam o mesmo tint e a borda escureceria junto com o corpo.

  O `IconBorder` do card e o `SlotBorder` do slot seguem desativados: a placa do ícone já traz o próprio aro, e dois contornos concorrentes sujavam a leitura.

  > **Cuidado ao gerar um sprite "só borda":** o alpha é `alphaDoContorno − alphaDoMioloEncolhido`. Escrever `outer − (1 − hole)` (que foi a primeira tentativa) devolve `1` no meio da peça — ou seja, um retângulo **cheio** em vez de um anel, que na tela apareceu como uma chapa clara cobrindo o card inteiro.

- **Madeira como motivo estrutural.** `WoodPanelFrame9Slice.png` (molduras dos 3 painéis) e `WoodCardFrame9Slice.png` (cards). O grão é procedural: ruído *value noise* deformando um padrão senoidal de anéis, com a direção do grão alternando conforme a face da moldura (`Mathf.Abs(p.x) < Mathf.Abs(p.y)` decide grão horizontal ou vertical), de modo que as barras horizontais e verticais não repitam o mesmo desenho.
- **Regra do 9-slice para qualquer detalhe desenhado na moldura:** num `Image.Type.Sliced`, só as **quadrículas de canto** são desenhadas em tamanho nativo — as faixas entre elas são **esticadas**. Portanto qualquer elemento com forma reconhecível (runa, rebite, ornamento) precisa ficar **dentro da região de borda** (`spriteBorder = 56`), senão vira um borrão esticado. Elementos que podem ocupar as faixas centrais são apenas os que não sofrem com esticamento: linhas retas paralelas à borda e o grão da madeira.
- **Descartado (19/08):** chegou a existir uma **moldura dupla** (segunda linha fina recuada ~31px) com **4 rebites** nas quinas. Foi removida a pedido do usuário. Se voltar a ser desejada, é gerada na própria arte, não como objetos de cena.
- **Ornamentos** (`Assets/UI/Theme/Ornaments/`): `CornerBracket.png` (cantoneira em "L", nos 4 cantos de cada card — a rotação por canto é feita via `localEulerAngles.z` 0/-90/180/90 do mesmo sprite), `TitleDiamond.png` (losango sólido usado nos títulos).
- **Runas entalhadas na moldura.** As runas não são objetos de cena: são gravadas na própria arte do `WoodPanelFrame9Slice`. São **8 no total, posicionadas dentro das regiões de canto** (duas por canto — uma no braço horizontal, outra no vertical), justamente por causa da regra do 9-slice acima: na primeira versão ficavam no meio de cada face e eram esticadas pelo Sliced, virando borrões — foi por isso que pareciam "apagadas", não por falta de contraste. O efeito de entalhe vem de dois passes: o glifo em tom queimado (`burnt`, quase preto) mais uma cópia deslocada `(1.3, -1.3)` em tom claro mostrando só a borda, simulando a luz na quina do sulco. Ficam mascaradas por `onWood` para nunca invadirem o miolo do painel. Isso é o oposto da tentativa rejeitada anteriormente (runas azuis flutuando ao lado do título): entalhadas, pertencem à madeira em vez de competir com o conteúdo.
- **Título em faixa de largura total** (estilo Megabonk): cada `TitleContainer` recebe um filho `TitleBand` (`LayoutElement.ignoreLayout = true`, `SetSiblingIndex(0)`, stretch total) usando `TitleBand9Slice.png` — fundo navy claro com uma linha de madeira na aresta inferior, separando o título do conteúdo. O texto leva um `Shadow` escuro deslocado (2, -2) para parecer entalhado.
- **Recuo do conteúdo em relação à moldura:** os 3 painéis usam `padding = 22` no próprio `VerticalLayoutGroup`. Sem isso, qualquer filho de largura total (como a faixa de título) é desenhado **por cima da moldura de madeira**, escondendo a borda. Os paddings dos containers internos (`CardsContainer` 38, `ButtonsContainer` 38, `EntitiesContainer` 4) já descontam esses 22 para manter o espaçamento efetivo de antes.
- **Descartado:** a composição régua + losango (`RuleLeft`/`DiamondLeft`/`Title`/`DiamondRight`/`RuleRight`) e a faixa chanfrada `TitleBanner.png` foram substituídas pela faixa de largura total. `TitleDiamond.png` segue no repo sem uso.
- **Motivos testados e descartados (19/08):** chegaram a ser gerados e aplicados ornamentos de **engrenagem** (`GearLarge.png`/`GearSmall.png`, cantos dos painéis) e **runas** (`RuneA/B/C.png`, ladeando os títulos), além dos losangos anteriores (`CornerDiamond.png`). Foram removidos da cena a pedido do usuário por não agradarem visualmente — os PNGs continuam no repo, mas **nenhum é referenciado**. Hoje os cantos dos painéis ficam limpos, só com a moldura de madeira. Não reintroduzir esses motivos sem pedido explícito.

  **Regra estrutural (importante):** ornamento **nunca** entra na arte do 9-slice — se entrasse, esticaria/repetiria junto com o painel e deformaria. Cada ornamento é uma `Image` filha própria, com `LayoutElement.ignoreLayout = true` (pra escapar do `VerticalLayoutGroup`/`HorizontalLayoutGroup` do pai), `raycastTarget = false`, âncora no canto correspondente e `sizeDelta` fixo. Divisão: **painel/card = 9-slice (estica)**, **ornamento = sprite simples (tamanho fixo, só ancorado)**. Mesma separação usada por Hades/Megabonk/Raveswatch.
- **`AbilitySelectionUI` (cena, `CanvasSkillSelector/SkillSelectionPanel`)** já reskinada diretamente na hierarquia: `SkillPanel`/`InventoryPanel`/`StatsPanel` usam `PanelFrame9Slice`, cards usam `CardFrame9Slice`, títulos em Lilita One, corpo em Fredoka; botões `Exilar`/`Pular`/`Atualizar` com preenchimento sólido por cor de função + `BorderOnly9Slice` tintado de bronze como filho `BorderOverlay` (`raycastTarget = false`) por cima; ícone dos cards (`ImageContainer/Background/Icon`) recolorido pra branco com `Outline` escuro, garantindo leitura em cima de qualquer cor de fundo de raridade (`RarityHelper.Color`). Camada decorativa: `OrnamentTopLeft/TopRight/BottomLeft/BottomRight` (losangos) nos 3 painéis, `TitleBanner` dentro de cada `TitleContainer` (`SetSiblingIndex(0)`, atrás do texto), e `BracketTopLeft/TopRight/BottomRight/BottomLeft` em cada card.
- **Regras de espaçamento** (aplicadas na `AbilitySelectionUI`, valem como referência pras próximas telas): o padding interno de qualquer container filho de um painel precisa ser **maior que a espessura da borda do 9-slice**, senão o conteúdo encosta visualmente na moldura. Espaçamentos em uso (apertados em 25/08 a pedido do usuário — a tela estava respirando demais):

  | Container | Espaçamento |
  |---|---|
  | `CardsContainer` (entre os 3 cards) | `spacing = 26`, `childForceExpandHeight = false` |
  | `EntitiesContainer` (entre seções do inventário) | `spacing = 10` |
  | `Weapons`/`Skills`/`Items` (título → régua → grade) | `spacing = 4`, padding inferior `4` |
  | `Container` de cada seção (entre slots) | `Grid.spacing = 6`, padding vertical `2` |

  > **`childForceExpandHeight` finge ser espaçamento.** O `CardsContainer` tinha `spacing = 0` e ainda assim havia ~49px entre os cards. Com `childControlHeight = false` e `childForceExpandHeight = true`, o layout **não redimensiona** os filhos: ele distribui a altura sobrando como folga dentro do slot de cada um e centraliza o card ali. Os 748px do container menos os 600px dos 3 cards viravam ~49px de ar por card, invisível no Inspector porque o campo `spacing` continuava zerado. Desligar a flag e usar `spacing` de verdade é o que torna o valor legível e ajustável. O teto confortável é ~34: o container tem 748px, menos 26 de padding em cima e embaixo sobram 696 úteis, e os 3 cards já ocupam 600 — acima disso eles encostam na moldura.
- **Botões**: o preenchimento usa `SolidRounded9Slice.png` (branco sólido, tintado por `Image.color`) com **o mesmo raio de canto** do `BorderOnly9Slice` usado no `BorderOverlay`. Os dois sprites precisam ter raio idêntico — se o preenchimento usar outro sprite (ex.: o `UISprite` padrão da Unity, quase reto), o preenchimento vaza pelos cantos da borda arredondada.
- **Slots de inventário**: `InventorySection` instancia os slots em runtime a partir de `InventoryUI.slotPrefab`, que aponta pro prefab de projeto **`Assets/Prefabs/UI/WeaponUI.prefab`** (não mais pra um objeto da cena) — é lá que se reestiliza o slot. Os antigos placeholders `Weapon1/2/3` que ficavam soltos dentro de `Weapons/Container` na cena foram removidos: eles apareciam em jogo junto com os slots reais, fingindo armas que o jogador não tinha. Hoje `Weapons/Container`, `Skills/Container` e `Items/Container` começam vazios e são `GridLayoutGroup` (célula `110x128`, spacing `6`, 3 colunas), preenchidos só em runtime.

  Estrutura do slot (`WeaponUI`): `SlotFrame` (**102×102**) contém `SlotShadow` (retângulo preto 55% deslocado (4, -4), dando a leitura de "encaixe" do slot), `SlotPlate` (a bandeja do slot, hoje em `#0C2238`), `BackGround` (a placa colorida por raridade, 92×92) com o `Pattern` e o `Icon` (68×68) dentro, e `SlotBorder` (`BorderOnly9Slice` em bronze, desativado). Abaixo do frame vem o `LevelLabel` (110×24), que mostra o nível no formato compacto do Megabonk (`InventoryEntry.LevelDisplay` = `LVL {CurrentRarity + 1}`): Fredoka **20 Bold**, **centralizado** sob o ícone, em `textTitle` (creme) com `Outline` quase preto para ler sobre qualquer fundo.

  > **A bandeja do slot precisou clarear (25/08).** `SlotPlate` era `#050F1C` (`panelRecess`), escolhido quando a placa de raridade ainda era clara — a bandeja escura servia de recesso atrás dela. Com a placa de corpo escuro (ver "Placa de ícone por raridade"), escuro sobre escuro deixou o slot sem contorno nenhum contra o painel; a queixa foi "o fundo do card está muito escuro, mal dá para ver". Hoje a bandeja é o campo `slotTray` do `UIThemeConfig` (`#0C2238`, obtido por `Lerp(panelBackground, panelSurface, 0.35)`), aplicado em runtime por `InventorySlotView.ApplyTheme`. A inversão de papéis (bandeja mais clara que a placa que ela segura) é intencional, não um resquício.

  > **O caminho até esse tom.** A primeira tentativa foi `panelSurface` cheio (`#112942`) e o retorno foi "ficou bem claro, queria mais escuro porém visível". `panelRecess` já tinha sido reprovado por sumir. O ponto de equilíbrio é ficar **pouco acima** do fundo do painel: o slot precisa apenas de um degrau perceptível, porque quem carrega a leitura da peça é o aro colorido da placa, não o preenchimento da bandeja. Se um dia a paleta mudar, recalcule pelo `Lerp` em vez de copiar o hex.

  > **O slot não era quadrado, e por isso o ícone parecia torto.** O `VerticalLayoutGroup` do slot tinha `childForceExpandWidth = true`, o que esticava o `SlotFrame` para os 110px da célula do grid enquanto a altura ficava presa em 104 — placa e ícone ficavam centrados num retângulo 110×104, mas a assimetria (somada à sombra deslocada (6, -8)) lia como desalinhamento. Com a flag desligada e `preferredWidth = 102` no frame, tudo fica quadrado e concêntrico. Pelo mesmo motivo o `LevelLabel` precisou de `preferredWidth = 110` explícito: sem `childForceExpandWidth`, todo filho depende do próprio `LayoutElement` para ter largura.

  `InventorySlotView` acha `Icon`/`LevelLabel`/`BackGround` por **busca recursiva** (`FindDeep`), não por caminho fixo, justamente pra a arte do slot poder ganhar níveis de aninhamento (shadow/plate) sem quebrar o script. O fundo do slot (`BackGround`) **é pintado com a cor de raridade em runtime** por `InventorySlotView.Apply` (`_rarityBorder.color = entry.RarityColor`), então qualquer cor definida nele no Inspector serve só de preview no Editor e é sobrescrita em jogo. A mesma chamada acha o `Pattern` e aplica o brilho da raridade (ver "Brilho de fundo por raridade"). O `LevelLabel` usa `ApplyBodyHighlight` (Fredoka + `textTitle`), não `ApplyBody`: em `textBody` o nível ficava apagado demais no meio da grade. Desde 25/08 o corpo da placa é escuro e o ícone é branco, então o ícone lê por contraste direto; ele usa `preserveAspect = true` + `Outline` escuro (`#0A0705`, alpha 0.85), o mesmo do card.
- **Cantoneiras de seleção do card**: os 4 `Bracket*` ficam agrupados sob um filho `SelectionBrackets` (stretch no card, `LayoutElement.ignoreLayout = true`, sem `Image` própria pra não bloquear raycast) que **começa desativado**. `AbilityCardUI` implementa `IPointerEnterHandler`/`IPointerExitHandler` e liga/desliga esse container, então as cantoneiras marcam apenas o card sob o cursor — é indicador de seleção, não decoração fixa. `OnDisable` e `Setup` forçam o estado oculto para o card não reaparecer marcado ao ser reciclado. Os brackets em si têm `raycastTarget = false`, senão sairiam por fora do card e roubariam o hover.
- Pendente de propagar pro resto da UI (`ShopSlotUI`, `SellInventorySlotUI`, `ChestRevealEffect`, `InteractPromptUI`) — o `TooltipUI` e a UI de escolha de caminho (`ChooseWayScreenUI`/`ChooseWayTotemBadge`, ver 4.13) já nascem no tema; hoje `AbilitySelectionUI` (incluindo card de habilidade, slot de inventário e linha de status), `TooltipUI` e a UI de escolha de caminho seguem o tema; as demais telas ainda usam cor/fonte fixas do Inspector.

**Onde mora a arte de cada tela (26/08).** `Assets/UI/Theme/` é a pasta **compartilhada**: só entra ali o que mais de uma tela usa (`OrnateFrame9Slice`, `ButtonPlate9Slice`, `CardPlate9Slice`, ornamentos, `Rarity/`). Arte que existe para **uma tela só** ganha pasta própria por tela — hoje `Assets/UI/ChooseWay/` (placa dos totens). Ao criar uma tela nova com arte exclusiva, crie a pasta dela em vez de despejar em `Theme/`, e mantenha o gerador correspondente em `Assets/Editor/`.

### 4.11 Cenas

**O que é / ideia central:** organização de cenas do projeto — hoje o jogo roda inteiro numa única cena de gameplay, sem separação entre menu, mapas diferentes ou tela de game over (consistente com a lacuna de "sem `GameManager`"/"sem múltiplos mapas" da seção 6).

- `Assets/Scenes/SampleScene.unity` — cena principal (com NavMesh bakeado).
- `Assets/Scenes/TestScene.unity` — cena de teste.
- `Assets/_Recovery/` — lixo de auto-recovery do Editor, **não são cenas de gameplay organizadas**.

### 4.12 Progresso da run em runtime (posse e nível) — `Assets/Scripts/Player/Skills/`, `Assets/Scripts/Cart/`

**O que é / ideia central:** é onde mora a resposta para "o que o jogador já pegou nesta run e em que nível". Antes esse estado ficava gravado dentro dos próprios ScriptableObjects (`SkillDefinition.currentRarity` etc.), o que causava um bug crítico: como SO é um asset em disco e instância única, o progresso **persistia entre sessões de Play** — o jogador maximizava tudo, parava o jogo, iniciava de novo e o orbe não oferecia mais carta nenhuma, porque para o `AbilityDrawer` todas as skills já estavam no nível máximo. Desde 25/08 o SO é dado de design imutável e o progresso vive em dicionários runtime dentro dos handlers, que morrem junto com a run.

**Regras:**
- Nenhum ScriptableObject (`SkillDefinition`, `WeaponDefinition`, `WeaponSkillDefinition`) guarda posse, nível ou exílio. Eles expõem só `MaxRarity`, `LevelCount` e as tabelas de nível (`GetLevelForRarity`/`GetStatsForRarity`).
- **Nível "não adquirido" é `-1`.** `GetRarity` devolve `-1` para qualquer coisa que o jogador ainda não pegou; nível `0` é a primeira raridade de verdade (Common). Quem exibe raridade deve usar `Mathf.Max(rarity, 0)`.
- `PlayerSkillHandler` é dono do progresso de **skills**; `PlayerCartWeaponHandler` é dono do progresso de **armas e weapon skills** (nível de cada, vínculo arma↔skills aplicadas, e o cache de stats efetivos).
- Os **stats efetivos** de uma arma (base + `WeaponSkillDefinition` aplicadas) são calculados por `PlayerCartWeaponHandler.GetEffectiveStats(weapon)`, com cache por arma invalidado a cada aquisição/upgrade/skill aplicada. Não existe mais `WeaponDefinition.GetEffectiveStats()`.
- `IDrawable` **não** expõe mais `CurrentRarity` — a interface é só `DisplayName` + `Icon`. Raridade é sempre um parâmetro explícito, passado por quem sabe o estado, porque o mesmo `IDrawable` significa coisas diferentes conforme o dono (item tem raridade fixa de design, skill/arma têm nível de run).
- Os dois handlers expõem `Instance` estático (mesmo padrão de `ShopManager`/`SellManager`/`SplineRuntimeState`), para os poucos consumidores que não têm como receber o handler por referência (`TooltipBuilder`, que é estático, e `AbilityCardUI`).
- `ResetForNewRun()` em ambos limpa dicionários, listas de adquiridos, exílios e cache. **Ainda não há quem chame** — hoje o estado zera naturalmente porque morre com o GameObject ao sair do Play. Quando o `GameManager` existir (lacuna da seção 6), é ele quem deve chamar os três `ResetForNewRun` (skills, armas, itens) ao iniciar uma run.

> **Por que o bug era invisível no Editor até maximizar tudo.** Com progresso parcial, o drawer ainda achava candidatos (as skills não maxadas), então parecia que só "vinham menos opções". Só ao maximizar todas é que `candidates` ficava vazio e a tela aparecia sem carta nenhuma. O sintoma era intermitente; a causa era determinística.

> **Resíduo em disco.** Os `.asset` de `Assets/Resources/Skills`, `Weapons` e `WeaponSkills` tinham a chave `currentRarity` gravada (as três skills estavam em `currentRarity: 4`, prova do bug). Como o campo não existe mais na classe, o Unity ignoraria a chave órfã, mas ela foi removida do YAML na mesma tarefa para o repo não carregar estado de run versionado.

- **`Player/Skills/PlayerSkillHandler.cs`** — `Dictionary<SkillDefinition,int> _rarityBySkill`; API: `GetRarity`, `HasSkill`, `CanLevelUp`, `NextRarity`, `ApplySkill`, `ExileSkill`, `IsExiled`, `ResetForNewRun`.
- **`Cart/PlayerCartWeaponHandler.cs`** — `_rarityByWeapon`, `_rarityByWeaponSkill`, `_skillsByWeapon`, `_effectiveStatsCache`; API: `GetRarity` (sobrecarregado para arma e weapon skill), `HasWeapon`, `HasWeaponSkill`, `CanUpgrade`, `CanLevelUp`, `NextRarity`, `AcquireWeapon`, `UpgradeWeapon`, `ApplyWeaponSkill(weapon, skill, rarity)`, `GetAppliedSkills`, `GetEffectiveStats`, `GetCurrentStats`/`GetNextStats`, `ResetForNewRun`.
- **Consumidores ajustados**: `AbilityDrawer` (pergunta ao handler em vez de ao SO), `AbilitySelectionUI` (`_weaponHandler.ApplyWeaponSkill(...)`, já que a arma não se auto-modifica mais), `AbilityCardUI` (`GetCurrentStats`/`GetNextStats` via `Instance`), `ArrowWeaponController` (`_weaponHandler.GetEffectiveStats<ArrowLevelData>(weapon)`), `InventoryUI`/`InventoryEntry` (raridade passada no construtor), `InventorySlotView`/`TooltipTrigger`/`TooltipBuilder` (raridade explícita em `SetSource`/`Build`), `Assets/Editor/WeaponSkillDefinitionEditor.cs` (parou de desenhar o campo removido).

### 4.13 UI de escolha de caminho — `Assets/Scripts/UI/ChooseWayUI/`, `Assets/Scripts/Splines/SplinePathParticles.cs`, `Assets/Scripts/Totem/TotemRegistry.cs`, `Assets/UI/ChooseWay/`

**O que é / ideia central:** é a tela que aparece quando o jogador para numa bifurcação e aperta E. É **híbrida, não world-space nem tela cheia**: cada totem bloqueado ganha um **selo pequeno** flutuando acima dele (crachá + custo, só "onde"), e o **conteúdo detalhado** (nome, descrição, custo, saldo, botão) mora numa **barra na parte inferior da tela** (só "o quê"). A seleção no mundo é reforçada por **partículas correndo ao longo do trilho** escolhido, na direção do destino, mais o totem correspondente acendendo — redundância proposital para o jogador nunca precisar ler texto para saber para onde vai.

**Substituiu a versão anterior (26/08)**, uma placa holográfica world-space (borda com bloom, feixe de luz, faíscas). Foi trocada a pedido do usuário porque destoava do 3D: todo o jogo usa **um único material flat-shaded** (`Assets/Meshes/Common.mat`, ver 4.10/estilo de arte abaixo) sem brilho nenhum, e a URP tem Bloom global com `threshold 1` — a placa antiga era a única coisa da tela que "floreceava". A troca não foi só "menos brilho": é uma metáfora de **matéria** (madeira pintada, chanfrada) no lugar de **luz**.

**Regras:**
- Ao entrar na zona (antes mesmo de apertar E), os **selos de todos os caminhos bloqueados da bifurcação já aparecem**, em repouso — o jogador vê os destinos possíveis sem precisar abrir o menu.
- Só o caminho **selecionado** tem a barra inferior visível e o selo em destaque (maior, cor cheia, balançando); os outros ficam em repouso (escala normal, cor escurecida).
- Trocar de seleção (A/D, ←/→, analógico, ou as setas da barra) faz **três coisas ao mesmo tempo**: troca o selo em destaque, troca o conteúdo da barra (com um "pop"), e migra as partículas do trilho para a nova spline.
- As setas de navegação da barra somem nas pontas da lista (primeiro/último caminho), igual à versão anterior.
- A cor de identidade de cada caminho (`SplineEntry.themeColor`) pinta a faixa de título do selo, o escudo da barra e as partículas do trilho. **Branco não configurado cai no acento do tema** (`UIThemeConfig.panelBorder`), mesma regra de antes. O botão `Desbloquear` **não** segue o acento — é sempre `actionPrimary` (Copper).
- Sem moedas suficientes, o custo vira `actionDestructive` e o botão fica não-interativo.
- **A barra não mostra o saldo de moedas.** Chegou a mostrar ("Você tem N"), mas foi removido a pedido do usuário (27/08): o saldo já é responsabilidade da UI central do jogo, e repetir aqui só polui a barra.
- Enquanto o menu está aberto a câmera **reenquadra**: afasta e sobe o assunto na tela, para a barra inferior não ficar por cima do trilho selecionado. Ela volta ao enquadramento normal ao fechar.
- Toda animação roda em `Time.unscaledDeltaTime` (o menu pausa o jogo) — inclusive a da câmera, que para isso liga `CinemachineBrain.IgnoreTimeScale` durante o menu.

**Vocabulário visual "madeira pintada" (o que faz virar cartoon low poly, não mais holograma):**
- **Cantos chanfrados a 45°**, nunca arredondados — é a assinatura poligonal do resto do jogo.
- Cor **chapada, sem gradiente contínuo**. A placa da barra chegou a ter degradê no topo e lábio escuro embaixo (imitando o `ButtonPlate9Slice`), mas o usuário apontou (27/08) que isso a fazia parecer um botão gigante — hoje a face é **uma cor só** e quem delimita a peça é exclusivamente o contorno.
- **Sem ruído, sem textura de detalhe** — o 3D do jogo também não tem (`Common.mat` não tem normal map nem textura de detalhe, só a paleta `ImphenziaPalette02-Albedo`).
- **Contorno escuro uniforme** (~5px) em toda peça — é ele, e só ele, que dá a leitura de "objeto sólido".
- **Texto sobre madeira usa `woodOutline`**, não as cores frias de texto do tema (`textBody`/`textMuted`): sobre o bege da placa elas ficam lavadas e ilegíveis. Foi a queixa de 27/08 sobre a descrição.
- Sombra = silhueta chapada escura deslocada (6, −8), sem blur.
- **Nenhuma cor acima de 1.0** — o Bloom global tem `threshold 1`; qualquer HDR aqui volta a florescer.
- As cores de madeira foram **amostradas da própria `ImphenziaPalette02-Albedo.png`** (os swatches que pintam o mundo), não inventadas: viraram os campos `woodLight`/`woodFace`/`woodMid`/`woodDark`/`woodOutline` do `UIThemeConfig` (seção 4.10).

**Selo world-space por totem — placa de uma linha (27/08)** (Canvas `380×200`, escala `0.0085`, `3.5` unidades acima do totem, rotação `(30, 45, 0)`).

**Terceira versão do selo, e a que ficou.** A primeira era um escudo octogonal com uma pílula de custo pendurada; a segunda virou um cartão `400×256` com faixa de título colorida, ícone em crest, descrição e custo. A queixa do usuário sobre a segunda foi direta — "muito grande e difícil de ler" — e a referência que ele trouxe (dois selos de destino de um jogo mobile, empilhados sobre o totem) é **uma placa de uma linha só**: ícone à esquerda, nome em cima, custo embaixo, tudo dentro de um retângulo baixo. O fundo escuro da referência **não** foi adotado a pedido dele: "o fundo não precisa ser escuro, apenas siga o posicionamento do layout" — a madeira continua, muda só o arranjo.

Estrutura (`Card` `330×128`, ancorado logo acima do `Tail`):

| Peça | Papel |
|---|---|
| `Tail` (`BadgeTail`) | triângulo de madeira apontando para baixo — é o que amarra a placa àquele totem |
| `Shadow` (`SignShadow9Slice`) | silhueta chapada deslocada `(5, -6)` |
| `Fill` (`SignPlate9Slice`) | o corpo de madeira, `pixelsPerUnitMultiplier = 1.5`. **É o mesmo sprite da barra inferior** |
| `Border` (`SignBorder9Slice`) | anel chanfrado **na cor do caminho**, por cima do fill — hoje é ele que carrega a identidade do destino |
| `Icon` | `themeIcon` do destino, `88²`, **tintado com a cor do caminho**, com `Outline` escuro |
| `Title` | nome em Lilita One, caixa alta, autosize 20–30, em `woodOutline` |
| `CostRow` | moeda `34²` (com `Outline` escuro) + custo em Nunito **Bold 34**, logo abaixo do título — sem pílula atrás |

> **Ícone e cadeado são o que o jogador lê primeiro, então são as peças grandes (27/08).** A primeira montagem deste layout saiu com ícone `56` e cadeado `24` — proporções de texto, não de símbolo — e a leitura em jogo continuou ruim. Hoje o ícone ocupa `88` (quase toda a altura útil da placa) e o cadeado `38`, ambos com `Outline` escuro por cima da madeira. É o inverso da intuição de "enxugar": encolher a placa **e** encolher os símbolos junto anula o ganho, porque o que identifica o caminho à distância é o desenho, não o nome. A placa cresceu de volta para `330×124` só para caber os dois — ainda menos da metade da área do cartão de `400×256` que começou tudo.

> **O cadeado saiu de vez (27/08).** Ele passou por plaquinha escura, glifo pequeno e glifo grande com contorno, e em nenhuma das versões o usuário achou que ficou bom em jogo — a última reprovação foi na captura em gameplay. Foi removido do selo. Não é perda de informação: **todo caminho que ganha selo é bloqueado por definição** (o selo só aparece para spline bloqueada), então o cadeado repetia o que a própria existência da placa já dizia. O sprite `LockPath` continua em uso na barra inferior. Não reintroduzir no selo sem pedido explícito.

> **Moeda e custo: o problema era contraste de matiz, não de brilho (27/08).** O ícone da moeda é tintado com `panelBorder` (Copper `#BC621B`) e ficava sobre a madeira (`#C69F7A`) — **duas cores quentes de brilho parecido**, então a moeda praticamente desaparecia na placa. A correção é o `Outline` escuro por baixo dela, que recorta o disco sem trocar a cor de papel. Já o número era só questão de tamanho: subiu de `24` para `34` **Bold**, e a linha de custo de `30` para `42` para caber. Regra geral para esta placa: **texto e símbolo sobre madeira precisam de contorno escuro ou de cor escura — nunca de outra cor quente crua.**

> **A cor do caminho migrou da faixa para a borda e o ícone.** Sem faixa de título, o acento precisava de outro lugar para viver. Ele agora pinta o anel `Border` (`accentPlate` do componente) e o próprio `Icon` — que é justamente o que a referência faz, e é a mesma regra do card de habilidade da seção 4.10: **peça escura/neutra, borda viva**. `ChooseWayTotemBadge.SetSelected` tinge os dois, então o selo em repouso perde cor no anel **e** no ícone ao mesmo tempo.

> **Texto sobre madeira é escuro, não creme (27/08).** `ApplyTheme` aplicava `ApplyTitle`/`ApplyStatValue` no nome e no custo, o que os deixava em `textTitle` (creme) — cor pensada para quando o texto ficava sobre a faixa colorida. Direto sobre a madeira clara, creme some. Hoje nome, custo e cadeado são repintados em `woodOutline` depois dos `Apply*`, e o ramo "tem moedas" do `Bind` também. O ramo "não tem moedas" segue em `actionDestructive`, que continua legível sobre a madeira.

> **Peças que sumiram do selo, mas continuam no componente.** `crest`, `lock_`, `costPill` e `descriptionText` não são mais ligados pelo builder — a descrição vive só na barra inferior, e o crest/pílula/plaquinha de cadeado não existem neste layout. Todos os campos do `ChooseWayTotemBadge` são null-safe, então isso não quebra nada; se um layout futuro voltar a usá-los, é só religar em `BuildBadge`. Os sprites `LockPlate`, `CostPill9Slice`, `AccentPlate9Slice` e `Shield` seguem no repo — o `Shield` ainda é usado pela barra inferior.

> **`SerializedObject.FindProperty` devolve `null` em silêncio — e o builder não checava (corrigido em 27/08).** Uma versão anterior do `BuildBadge` gravava um campo `shield` que não existia mais no componente: o `FindProperty` devolvia `null` e a linha seguinte estourava `NullReferenceException` **no meio** do `BuildBadge`, então o `viewData.badge` nunca era atribuído e rodar o menu deixava **todos** os totens com o selo desligado. Hoje toda atribuição passa por um helper `Assign(data, nome, valor)` que loga um warning nomeando o campo em vez de derrubar a reconstrução — um campo renomeado no futuro custa um aviso, não um selo mudo.

> **Chanfro total vira losango.** `LockPlate` era gerada com `chamfer = 28` num sprite de `64` — o corte das quatro quinas se encontrava no meio e a plaquinha saía como um diamante. Caiu para `12`. Regra: o chanfro precisa ficar bem abaixo de metade da menor dimensão do sprite, senão a peça deixa de ser um retângulo chanfrado.

> **Repouso vs. selecionado** (`ChooseWayTotemBadge.SetSelected`): anel e ícone vão da cor cheia do caminho a `Lerp(acento, cinza, restingBlend = 0.55)` e o corpo da placa escurece por `restingCardBlend = 0.3`, além do crescimento com overshoot do `ChooseWayBadgeAnimator`.

**Barra inferior** (`BottomBar`, `1400×240`, âncora bottom-center, `anchoredPosition (0, 48)`, dentro do Canvas `CanvasChooseSpline` — reaproveitado da cena, screen-space overlay, `1920×1080`):

| Coluna | Conteúdo |
|---|---|
| `CrestColumn` (220) | `Shield` 150² + ícone do destino |
| `InfoColumn` (740) | título (Lilita One, caixa alta) → descrição (Fredoka) → linha de `Pips` (um por caminho da bifurcação, o atual cheio) |
| `CostColumn` (320) | moeda + custo em Nunito, **sem moldura atrás** → botão `Desbloquear` |
| `PreviousButton`/`NextButton` | discos chanfrados (`ChevronPlate`) fora do layout, nas laterais da barra |
| `Hints` | `[A/D] Trocar · [Enter] Desbloquear · [E] Sair`, **fora da placa**, 46px abaixo dela |

> **A largura das colunas é um orçamento fechado — estourar comprime todas (28/08).** O `Content` é a barra menos `sizeDelta {-60, -32}` (altura reduzida de `-60` para `-32` em 31/08, ver nota de folga vertical abaixo), ou seja **1340×208 úteis**. As três colunas têm `flexibleWidth = 0` e `minWidth = -1`, então quando a soma `220 + 740 + 320` mais `2 × spacing 16` passa do box, o `HorizontalLayoutGroup` **encolhe as três proporcionalmente** — e o título, que cabia, passa a quebrar. Era exatamente o que acontecia com a barra em `1240×260`: as colunas pediam 1232 num box de 1180 e a `InfoColumn` recebia ~631 dos 660 pedidos. **O sintoma aparece na fonte, mas a causa é geométrica** — ao mexer em qualquer largura de coluna, refaça a soma antes de culpar o tamanho do texto.

> **Folga vertical para a fumaça do trilho não ficar atrás da barra (31/08).** A `BottomBar` ofuscava o trecho do trilho selecionado (`SplinePathParticles`) quando ele passava pela parte de baixo da tela — problema estrutural, já que a barra é Screen Space Overlay e é sempre composta por cima de qualquer coisa 3D, sem exceção possível (não dá pra fazer a fumaça "furar" a UI). A correção foi puramente 2D: `sizeDelta.y` da `BottomBar` caiu de `280` para `240`, **sem** mexer em `anchoredPosition.y` — como o pivot é `(0.5, 0)` (base fixa perto do fundo da tela), subir a posição deslocaria a faixa ocupada inteira pra cima (podendo piorar a oclusão do trecho de trilho mais próximo da câmera); reduzir a altura encolhe só o topo da faixa. O padding do `Content` acompanhou (`-60` → `-32` na vertical, mantendo `-60` na horizontal), preservando ~10px de folga em cima e embaixo do conteúdo (`188px` de conteúdo em `208px` úteis) — dentro da margem seguindo a mesma lógica de "orçamento fechado" documentada acima.

> **A barra deixou de disputar espaço com a fumaça: a câmera abre lugar pra ela (31/08, segunda passada).** Encolher a `BottomBar` (nota acima) reduziu a faixa ocupada, mas não resolveu — a fumaça percorre a spline **inteira**, então qualquer borda da tela onde a UI estivesse acabaria sendo atravessada em algum caminho. Foi por isso que a ideia de mover a barra para o canto direito foi descartada: numa câmera isométrica a 45° os trilhos correm nas **quatro diagonais**, então um painel à direita só troca quais caminhos ficam tapados. A correção que ficou é **de câmera, não de layout**: `ChooseWayCameraFraming` afasta a câmera ao abrir o menu (`zoomOut = 1.25` sobre o `OrthographicSize`) e faz um **pan de tela** que sobe o assunto, liberando a faixa de baixo. Em vez de tirar UI da frente do mundo, tira o mundo de trás da UI.

> **Fade da barra sobre a fumaça: implementado e descartado (01/09).** Chegou a existir uma rede de segurança em `ChooseWayScreenUI.LateUpdate`: se a spline selecionada cruzasse o retângulo de tela da `BottomBar`, um `CanvasGroup` baixava o alpha da barra para `0.45` e o devolvia quando o trilho saía de baixo. Funcionava, mas o usuário testou em jogo e o veredito foi claro — **barra translúcida lê como "UI desabilitada"**, não como transparência intencional; a fumaça aparecendo por trás não vale esse preço. Removido junto: os campos `barGroup`/`trailOverlapAlpha`/`trailFadeSpeed`/`trailSamples`/`trailRectPadding`, o `TrailCrossesBar()` e o par `HasPath`/`TrySampleWorld` do `SplinePathParticles`, que só existia para alimentá-lo. **A barra é sempre 100% opaca.** Se a fumaça passando por baixo voltar a incomodar, o único lever é o enquadramento — subir `screenPan`/`zoomOut` no `ChooseWayCameraFraming` —, nunca a opacidade.


> **O título não pode quebrar, por design (28/08).** `Title` usa `TextWrappingMode = 0` (quebra **desligada**) mais autosize `32–52` e `LayoutElement.preferredHeight = 70`. Nome de destino em duas linhas estourava os 60px do `LayoutElement` e, com `VerticalAlignment` Middle, transbordava para cima **e** para baixo, cobrindo a descrição. Com a quebra desligada, um nome longo demais encolhe até 32pt em vez de partir a linha. Vale a mesma condição registrada no card de habilidade (seção 4.10): o autosize só age porque `flexibleHeight = 0` e a altura preferida estão travadas — se a linha puder crescer, o TMP entende que o texto coube e nunca encolhe.

> **A coluna de texto é centralizada, não alinhada ao topo.** `InfoColumn` usa `ChildAlignment = MiddleLeft` e `spacing = 10`: o conteúdo (`70 + 10 + 76 + 10 + 22 = 188`) fica centrado nos 208 úteis (220 antes da folga vertical de 31/08), com ~10px de folga em cima e embaixo. Com `UpperLeft` (como era até 28/08) toda a sobra ia para o rodapé e os textos ficavam grudados na aresta de cima da placa.

> **As dicas são o único texto desta tela que NÃO fica sobre madeira (28/08).** O `Hints` é filho de `BottomBar`, mas ancorado à borda inferior com `pivot.y = 0` e `anchoredPosition (0, -46)` — ele desenha **abaixo** da placa (e da sombra dela, que tem offset `(6, -8)`), sobre o mundo escurecido pelo `screenDim`. Por isso a regra "texto sobre madeira é escuro" **não** se aplica a ele: `ApplyTheme` o pinta com `textTitle` cheio, e o GameObject carrega um `Outline` mais um `Shadow` que o recortam contra qualquer fundo de jogo. Antes ele usava `Lerp(woodOutline, textTitle, 0.45)` — um marrom-acinzentado pensado para madeira — em 22pt e sem contorno nenhum, e ficava praticamente invisível. Os dois componentes não precisam de cor configurada: o `foreach (var shadow in GetComponentsInChildren<Shadow>(true))` de `ApplyTheme` já aplica `outlineDark` no `Outline` e `textShadow` no `Shadow`.

**Divisão de componentes** (a antiga `ChooseWayPanelUI` acumulava os dois papéis; hoje são scripts separados):

- **`Scripts/UI/ChooseWayUI/ChooseWayScreenUI.cs`** — dono da barra inferior. Singleton no padrão de auto-ativação do projeto (getter `Instance` reativa o GameObject via `FindFirstObjectByType(..., Include)`), vive no `CanvasChooseSpline`. API: `ApplyTheme`, `Show(entry, position, total, affordable, coins, onUnlock, onPrevious, onNext)`, `Pop()`, `Hide()`.

- **`Scripts/UI/ChooseWayUI/ChooseWayScreenAnimator.cs`** — sobe a barra de baixo com overshoot (`back-out`, ~0.28s) na entrada; "pop" de escala rápido ao trocar de caminho.

  > **A troca de caminho não funcionava (bug corrigido em 27/08).** `Show()` chamava `PlayEnter()` **toda** vez, e `PlayEnter` começa jogando a barra para fora da tela (`_restPosition - riseDistance`) antes de animar a subida. Ao trocar de destino, o `Pop()` disparado logo em seguida fazia `StopCoroutine` na corrotina de entrada e rodava `PopRoutine`, que só mexe em `localScale` — a barra ficava parada fora da tela para sempre. O conteúdo até trocava, mas ninguém via, então parecia que a navegação estava quebrada. Hoje `Show()` só chama `PlayEnter()` quando a barra **ainda não estava visível**, e `PopRoutine` fixa `anchoredPosition = _restPosition` na primeira linha como rede de segurança. **Regra a levar adiante:** animação de entrada e animação de troca de conteúdo não podem compartilhar o mesmo slot de corrotina sem que a segunda restaure o que a primeira deixou pela metade.
- **`Scripts/UI/ChooseWayUI/ChooseWayTotemBadge.cs`** — dono do selo de um totem. API: `ApplyTheme`, `SetAccent`, `Bind`, `Show`, `Hide`, `SetSelected`. Sem botões — o canvas do selo não tem `GraphicRaycaster`. O `LateUpdate` faz **uma coisa só**: o sorting por profundidade descrito abaixo.

  > **O selo de um totem cobria o totem/partículas de outro (bug reportado 31/08, com print) — e o sorting por `Canvas.sortingOrder` sozinho NÃO resolve isso.** A nota antiga desta seção dizia que o selo, por ser pequeno, não precisaria mais do hack de sorting por distância que a placa holográfica anterior tinha (`SetCanvasSortOrder`, removido em 25/08) — **essa premissa era errada**, confirmado reproduzindo a cena do print no Editor: com dois totens próximos e um mais perto da câmera que o outro, o selo do totem mais perto acaba realmente ocupando, no espaço 3D, uma posição na frente do totem mais distante (ele flutuava `3.5` unidades acima do próprio totem, e nessa altura podia ficar mais perto da câmera que o totem vizinho) — **é oclusão de profundidade correta, não ambiguidade de ordem de desenho**. Confirmado empiricamente: aplicar um `Canvas.sortingOrder` diferente nos dois selos não mudou nada na sobreposição contra a *mesh* opaca do totem (`ZTest` contra opaco já funciona certo por si só). O sorting por profundidade (`LateUpdate`, `Canvas.sortingOrder = -depth * 10`, `depth` = profundidade ao longo do eixo de visão da câmera) foi reintroduzido mesmo assim — resolve o caso selo-vs-selo e selo-vs-partícula de outro totem (ambos Transparent/`ZWrite Off`, sem depth test confiável entre si).
  >
  > **Tentativa descartada (01/09): reposicionar o selo e escurecê-lo por oclusão.** Entre 31/08 e 01/09 o selo deixou de ficar em cima do totem e passou a ser recolocado todo frame por `LateUpdate` — subia `2` unidades em vez de `3.5` e deslocava `2.4` pro lado ao longo de `camera.transform.right`, com a `Tail` girando por `PointTailAt` pra continuar apontando pro totem. Em cima disso vieram duas camadas de correção automática: `ResolveOffsetSign` escolhia o lado (radial ao centroide da bifurcação, com desempate por contagem de oclusão) e `IsOccludingAnotherTotem` escurecia o selo em repouso que caísse sobre a silhueta de um totem mais distante. **Tudo isso foi removido a pedido do usuário: em jogo trouxe vários bugs, e o custo não se pagou.** O selo voltou a ser o filho `BadgeCanvas` do totem em `localPos (0, 3.5, 0)` — a posição vem da cena, não de código, e a `Tail` volta a apontar reto pra baixo (`localRotation` identidade).
  >
  > **O que caiu junto, por ter virado código morto:** os campos `totemAnchorHeight`/`sideOffset`/`occludingAlpha`/`occlusionFadeSpeed`, os helpers `PointTailAt`/`ResolveOffsetSign`/`ResolveRadialSign`/`CountOccludedTotems`/`BoundsOverlapsScreenRect`, o par `SetOwner`/`SetJunctionCenter` (e o centroide que o `JunctionTotemsController.Start` calculava para alimentá-lo), e o `TotemView.VisualBounds`, que só existia para essa checagem. `TotemRegistry.AllViews` ficou sem consumidor, mas continua no registro por ser API genérica.
  >
  > **A lição a levar adiante:** o que sobrava era caro em superfície (posição, rotação da haste, lado, opacidade — quatro coisas escritas por frame, cada uma podendo brigar com o animator ou com a corrotina de dismiss) para resolver um caso que só aparece em bifurcações com totens muito próximos. Se o problema voltar a incomodar, a correção deve ser **na cena** (afastar os totens, ou dar offsets locais diferentes aos dois `BadgeCanvas`), não em lógica que recalcula tudo em runtime.
  >
  > **Achado à parte, não relacionado ao bug em si: o campo `group` (`CanvasGroup`) dos dois selos da cena estava null.** `PlayDismiss` já dependia dele pra dar fade ao sumir (`if (group != null) group.alpha = ...`), então o fade de saída dos selos nunca rodou de fato — só o "subir e sumir" da posição. Corrigido adicionando `CanvasGroup` ao GameObject `Root` de cada selo e ligando o campo via Inspector (os dois selos da cena são objetos únicos, não prefab — não há uma segunda instância pra propagar o fix).
  >
  > **Armadilha de verificação: Play Mode via automação remota pode não avançar frames sozinho.** Testar essa mudança rodando o jogo de fato (em vez de simular a fórmula em Editor mode) mostrou `Time.frameCount` parado em `2` mesmo depois de mais de 100s reais — o Editor, controlado remotamente sem foco de janela/repaint contínuo, não estava ticando o Player Loop entre chamadas. `Canvas.sortingOrder` ficou em `0` (valor inicial) o tempo todo, mesmo com os objetos ativos e o script habilitado — sintoma de que `LateUpdate` simplesmente não rodava, não de um bug de lógica. Vale lembrar disso da próxima vez que uma mudança "não parecer ter efeito" num teste via automação: checar `Time.frameCount` antes de desconfiar do código.
- **`Scripts/UI/ChooseWayUI/ChooseWayBadgeAnimator.cs`** — balanço de placa pendurada (±2,5°, contínuo) e crescimento com overshoot ao ser selecionado.
- **`Scripts/UI/ChooseWayUI/ChooseWayCameraFraming.cs`** — reenquadra a câmera enquanto o menu está aberto. Singleton no mesmo padrão de auto-ativação, **mora no GameObject `Follow Camera`** (o que tem `CinemachineCamera` + `CinemachineFollow`): o `Awake` resolve os três componentes sozinho por `GetComponent` e pelo `CinemachineBrain` da `Camera.main`, então não há campo pra ligar à mão — só arrastar o script pro objeto certo (se ficar no objeto errado, ele loga um warning em vez de tentar adivinhar). Já está na cena (01/09). `SplineUnlockZone` chama `SetFocused(true/false)` ao lado do `FocusDimController`, e o `LateUpdate` interpola `_blend` com `Easing.CubicOut` em `transitionDuration = 0.35s`, aplicando `Lens.OrthographicSize *= zoomOut` e `FollowOffset -= panAxis * screenPan`.

  > **`brain.IgnoreTimeScale` tem que ser ligado, senão nada acontece.** O menu roda com `Time.timeScale = 0` e a cena tem o brain em `IgnoreTimeScale = 0` / `UpdateMethod = LateUpdate`: `GetEffectiveDeltaTime` devolve `Time.deltaTime`, que é **zero** enquanto pausado, e o damping do `CinemachineFollow` congela — mexer no `FollowOffset` não moveria a câmera um pixel. `SetFocused(true)` guarda o valor original e liga a flag; `SetFocused(false)` restaura. (O `Lens` não passa por damping, então o zoom sozinho funcionaria mesmo pausado — é o pan que depende disso.)
  >
  > **O pan é ao longo do `up` da câmera, não do `up` do mundo.** Como a projeção é ortográfica, deslocar a câmera pelo eixo `forward` não muda nada na tela e um deslocamento em `Vector3.up` do mundo teria uma componente inútil em profundidade. `ResolvePanAxis` captura `Camera.main.transform.up` na abertura do menu (a rotação é fixa, então vale pra sessão inteira) e o offset é **subtraído**: a câmera desce em espaço de tela, o que faz o assunto **subir** na imagem, que é o efeito desejado.
  >
  > **Os valores base são lidos no `Awake` e nunca reescritos.** `_baseOrthographicSize`/`_baseFollowOffset` são a origem da interpolação nos dois sentidos, e o `LateUpdate` sai cedo quando `_blend` já chegou no alvo — fora do menu o script não escreve nada no rig.
- **`Scripts/Totem/TotemView.cs`** — voltou a ser só o totem 3D: partículas, shake, pulso de desbloqueio, e **emissão de verdade** (o array `accentRenderers` recebe a cor do caminho via `MaterialPropertyBlock` — antes essa chamada existia mas estava comentada e não fazia nada). Delega toda a UI para o `ChooseWayTotemBadge` serializado (`badge`).
- **`Scripts/Totem/TotemRegistry.cs`** — registro estático `splineIndex → TotemView` (`Register`/`Unregister`/`TryGet`/`AllViews`). Ver "Bug de referências nulas" abaixo.
- **`Scripts/Totem/FocusDimController.cs`** — ganhou o padrão `Instance` (antes era referenciado por campo serializado em cada `SplineUnlockZone`; só existe um na cena).
- **`Scripts/Splines/SplinePathParticles.cs`** — **um único** `ParticleSystem` reutilizado (não um por spline), emissão 100% manual (`emission.enabled = false`, `shape.enabled = false`): `SetPath(container, splineIndex, reversed, accent)` guarda a cor e um cursor `t` que corre pela spline; `Update` avança o cursor e, a cada `1/puffsPerSecond` de tempo não escalado, emite uma **baforada** em cada um dos `trailCount` rastros defasados (`trailSpacing`), com posição de `SplineUtility.Evaluate`. `main.simulationSpace = World`, `main.useUnscaledTime = true` (o jogo está pausado). `StopPath()` limpa e para. O parâmetro `reversed` vem de `SplineUnlockZone`, que já sabe (via `GetClosestKnotIndex`) se o knot da bifurcação é o primeiro ou o último da spline de destino.

  **Fumaça cartoon (27/08).** A versão anterior emitia um quadrado chanfrado branco por frame de `Update`, o que dava um rastro de *faíscas* — o usuário pediu baforada de desenho, na linha da poeira sob o pé do personagem e da fumaça de fogueira das referências. O que mudou:

  | Aspecto | Antes | Agora |
  |---|---|---|
  | Sprite | losango chanfrado 32² | `TrailPuff.png`, **sheet 2×2** de nuvens (4 variações, ver gerador) |
  | Tamanho em cena | `0.35` fixo | `0.85–1.35` — os `0.55–0.95` da primeira tentativa sumiam contra o chão claro |
  | Cadência | 1 partícula por rastro **por frame** (dependia do FPS) | acumulador de tempo, `puffsPerSecond = 14` por rastro |
  | Tamanho | fixo `0.35` | `startSize` sorteado em `puffSizeRange` e crescendo por `sizeOverLifetime` (`0.45 → 1 → 1.35`) |
  | Opacidade | constante até morrer | `colorOverLifetime` entra em 14% da vida e desaparece no fim |
  | Movimento | só a tangente | tangente (`driftSpeed`) + subida (`riseSpeed`, com jitter) + espalhamento lateral (`sideScatter`) |
  | Cor | o acento cru do caminho | `Color.Lerp(acento, branco, whiteMix = 0.4)`, com jitter de brilho por partícula |

  > **Cada baforada sorteia um frame do sheet e uma rotação.** O `TextureSheetAnimationModule` fica em `Grid` 2×2 com `frameOverTime` **constante em 0** e `startFrame` aleatório `0–0.999`: isso faz a partícula escolher uma nuvem e **segurar** aquele frame a vida inteira, em vez de animar. Junto com `EmitParams.rotation` aleatório, é o que impede o rastro de virar o mesmo carimbo repetido — que é exatamente o que denuncia partícula de programador.

  > **A cor não é o acento puro.** Fumaça colorida cheia lê como bolha de energia, não como fumaça. Misturar 55% de branco mantém a identidade do caminho (é ela que diz "este trilho") e ainda assim lê como baforada. Mesmo raciocínio de `RarityHelper.GlowColor`.

  > **Os valores foram calibrados contra o trilho, não no vácuo.** A primeira rodada usava `puffSizeRange = 0.55–0.95` e `whiteMix = 0.55`: visto de longe, a fumaça ficava menor que a prancha do trilho e quase branca contra o chão claro do mapa. Subir para `0.85–1.35` e baixar o branco para `0.4` é o que faz a baforada ter peso e ainda dizer de que caminho ela é. Se o chão do mapa mudar de valor, esses dois números são os primeiros a revisitar.

  > **O material precisa ser transparente.** `TrailSparkMaterial.mat` (`Assets/UI/ChooseWay/TrailSparkMaterial.mat`) usa `Particles/Unlit` com `_Surface = 1`, `SrcAlpha/OneMinusSrcAlpha`, `ZWrite 0`, fila `Transparent` e a keyword `_SURFACE_TYPE_TRANSPARENT` — sem isso o alfa da nuvem seria ignorado e cada baforada apareceria como um **quadrado sólido**. Alpha blend, **nunca aditivo** — aditivo volta a florescer no Bloom global (`threshold 1`), que foi justamente o motivo de a placa holográfica antiga ter sido aposentada.
- **`Scripts/Splines/SplinePathVisual.cs`** — ganhou `ActiveRoot` (público) e um registro estático próprio (`TryGet(splineIndex, ...)`), preparando o terreno para a segunda fase (acender as pranchas via emissão, ver "Pendências" abaixo).
- **`Scripts/Splines/SplineUnlockZone.cs`** — `ShowSelected` chama `ChooseWayScreenUI.Instance.Show(...)` e `SplinePathParticles.Instance.SetPath(...)`; `MoveSelection` também dispara `ChooseWayScreenUI.Pop()`. Um novo método `RefreshRestingBadges` roda toda `Update()` (quando o menu não está aberto) para mostrar/esconder os selos de repouso conforme o jogador entra/sai do alcance de cada caminho.
- **Arte e montagem da cena (27/08 — geradores removidos).** Essa UI foi originalmente construída por dois Editor scripts, `Editor/ChooseWayArtGenerator.cs` (gerava os PNGs cartoon em `Assets/UI/ChooseWay/`) e `Editor/ChooseWayUIBuilder.cs` (montava `BadgeCanvas`, a barra e o sistema de partículas na cena). **Os dois foram removidos** a pedido do usuário: o projeto não usa mais Editor scripts para gerar UI/arte por código (ver seção 3) — daqui em diante a UI é montada/editada direto na cena e nos prefabs, e a arte (os PNGs abaixo) é editada diretamente. Os PNGs continuam em `Assets/UI/ChooseWay/` e seguem em uso normalmente: `SignPlate9Slice`/`SignShadow9Slice` (barra e cartão do selo), `Shield`, `AccentPlate9Slice`, `CostPill9Slice`, `BadgeTail`, `LockPlate`, `SignBorder9Slice`, `ChevronPlate`, `PipEmpty`/`PipFull`, `NailHead` (sem uso ainda), `TrailPuff` (sheet 2×2 das baforadas de fumaça do trilho). **Aposentados** (no repo sem uso): `WayPanelFill9Slice`, `WayPanelBorder9Slice`, `WayCrestFill`, `WayCrestRim`, `WayBeam`, `WayGlow`, `WaySpark`, `WayDisc`, `WayRule`, `TrailSpark`.

> **Regra de tamanho de 9-slice a manter em mente ao editar essas peças à mão:** `2 × spriteBorder` precisa ser menor que a menor dimensão em que a peça é desenhada — a pílula de custo (52px de altura) já deformou por causa disso quando `spriteBorder` estava em `30`; hoje está em `18`.

> **Por que nenhuma `SplineUnlockZone` fica "esquecida".** `JunctionTotemsController.OnEnable`/`OnDisable` registram/removem seus totens no `TotemRegistry` estático, e `SplineUnlockZone` consulta o registro em vez de depender de um campo serializado religado manualmente no Inspector (era assim antes, e só 1 das 9 zonas da cena tinha o campo preenchido). `FocusDimController` recebeu o mesmo tratamento (virou singleton).

**Pendências (não implementadas nesta passada, ficou de fora por escopo):**
- **Acender as pranchas do trilho selecionado.** A keyword `_EMISSION` já está ativa em `Common.mat` (mas com `_EmissionColor` preto); dá para aplicar `MaterialPropertyBlock` nos renderers de `SplinePathVisual.ActiveRoot` com um pulso viajando pela fila, sem instanciar material nem quebrar o SRP Batcher — mesmo padrão de `TotemView.ApplyEmissionTo`.
- `NailHead.png` é gerado mas não está posicionado nos cantos da barra/selo ainda.
- O `SplineManifest` (`Assets/Scripts/Splines/Manifest/SplineManifest.asset`) ainda tem a maioria das entradas em placeholder (`destinationName`/`description`/`themeIcon`); só os índices 1 e 2 foram preenchidos como exemplo real durante a validação desta feature.

> **DoF do foco desligado.** `Assets/Volumes/VPTotemFocus.asset` (o perfil que `FocusDimController` ativa ao abrir o menu) tinha `DepthOfField` com `focusDistance 3`/`gaussianEnd 4` — borraria justamente o trilho destacado pelas partículas, que fica a mais de 4 unidades da câmera. `active` foi setado para `0` nesse componente do perfil.

### 4.14 Celebração do desbloqueio de caminho — `Assets/Scripts/Splines/SplineUnlockSequence.cs`

**O que é / ideia central:** é o beat que roda quando o jogador confirma o pagamento de um caminho. Em vez de só confirmar a compra, o momento **mostra a recompensa**: o totem se prepara e descarrega, o trilho **se constrói em cascata** do totem até o destino, e só então o totem sai de cena e a barra fecha. Dura ~1,3s com o jogo ainda pausado.

**Por que existiu a reforma (28/08).** A versão anterior era um pulso senoidal de ±15% no totem por 0,5s e nada mais — o usuário não gostou. A causa não era a curva: **o desbloqueio não tinha recompensa, só confirmação.** Depois do pulso, tudo acontecia no mesmo frame — moeda debitada (com a barra já sumindo, então o jogador nunca via o custo ser pago), `Unblock` trocando o trilho quebrado pelo normal com um `SetActive` seco, o totem apagado por outro `SetActive(false)`, e o `CloseMenu` despausando. A coisa pela qual o jogador pagou — o caminho novo existindo — aparecia fora de cena, sem ninguém apontando para ela.

**Regras:**
- O desbloqueio é uma **sequência com beats**, não um efeito único, e roda inteira com `Time.timeScale = 0` — portanto **tudo** mede `Time.unscaledDeltaTime`, como o resto do fluxo de escolha de caminho.
- Durante a sequência o input de desbloqueio fica travado (`_unlocking`): `Update` retorna cedo, `E`/`Esc` não fecham o menu e `OnTriggerExit` é ignorado.
- A onda de construção sempre corre **do totem em direção ao destino**, nunca ao contrário.
- As moedas são debitadas **uma vez só**, no beat de pagamento, e o `Unblock` só acontece no fim — a spline não fica atravessável antes de a animação terminar.
- Todas as durações são `[SerializeField]`, para calibrar em jogo sem tocar em código.

**Beat sheet:**

| t (s) | O que acontece |
|---|---|
| 0.00 | `TotemView.PlayCharge` — antecipação: o totem encolhe para `0.92` |
| 0.12 | **Descarga**: estica para `1.10` com `BackOut`, `unlockBurstParticles.Play()`, emissão ao pico. O selo pisca a borda/ícone em branco e leva um punch |
| 0.30 | Moeda debitada. Barra: `-{custo}` sobe e some sobre a coluna de custo, carimbo `CAMINHO LIBERADO` na cor do caminho, botão desativado, punch de escala |
| 0.38–1.08 | **Onda no trilho**: `SplinePathVisual.PlayUnlockReveal` faz as pranchas surgirem em cascata, e a fumaça do trilho corre à frente da onda |
| 1.08–1.38 | `TotemView.PlayVanish` — o totem afunda no chão com escala Y → 0 e leve giro; o selo sobe e some |
| 1.48 | `Unblock()` + `CloseMenu()` — a barra desliza para baixo e o jogo despausa |

- **`Splines/SplineUnlockSequence.cs`** — orquestra o beat sheet. Fica no **mesmo GameObject** do `SplineUnlockZone` (`[RequireComponent]`), sem singleton nem fiação de cena. API: `Play(entry, view, reversed, onCoinsSpent, onComplete)`, `IsPlaying`.

  > **Por que a corrotina não mora no `TotemView`.** O `JunctionTotemsController` desativa o GameObject do totem no `Unblock`, o que **mata qualquer corrotina rodando nele**. O `SplineUnlockZone` fica ativo o tempo todo (e com `timeScale = 0` o jogador não consegue sair do trigger), então é o host seguro.

- **`Systems/Easing.cs`** — `BackOut`/`CubicIn`/`CubicOut` estáticos. O `BackOut` era privado dentro do `ChooseWayScreenAnimator`; foi extraído em vez de duplicado, e o animator passou a consumi-lo.

- **`Splines/SplinePathVisual.cs`** — ganhou `PlayUnlockReveal(reversed, duration)`. Ativa `normalRoot` antecipadamente, zera a escala de cada prancha e as faz crescer em cascata com `BackOut`, subindo `plankRiseHeight` do chão.

  > **A cascata é só a ordem dos filhos.** O `SplineTrackBuilder` instancia as pranchas como filhas diretas de `normalRoot`, **já em ordem ao longo do trilho** — não é preciso avaliar a spline. O `reversed` que o `SplineUnlockZone` já calculava para as partículas diz se a onda percorre os filhos do índice 0 para o fim ou o contrário, e é o que garante que ela sempre saia do totem. `ActiveRoot`/`TryGet` já existiam como API pública **sem nenhum chamador**, deixados prontos exatamente para isto.

  > **`Refresh()` não pode rodar no fim da revelação.** O `Unblock` só acontece depois, então naquele instante o `SplineRuntimeState` ainda diz "bloqueado" e um `Refresh()` reverteria o trilho para quebrado, desfazendo a animação. Por isso a corrotina termina sem reconciliar, e o `Refresh()` disparado pelo `OnSplineUnblocked` (que também sai cedo enquanto `_revealRoutine != null`) vira no-op.

- **`Splines/SplinePathParticles.cs`** — ganhou `PlayReveal(duration)`: reseta o cursor e o faz percorrer a spline inteira em `duration`, no lugar da `travelSpeed` fixa, para a baforada liderar a construção. Não reconfigura o `ParticleSystem` — `SetPath` já rodou na seleção do caminho.

- **`Totem/TotemView.cs`** — `PlayUnlockEffect` (o pulso senoidal) virou `PlayCharge()` + `PlayVanish()`, com a flag `IsVanishing` e o padrão de corrotina cancelável (`_routine` + `StopCoroutine`) que o componente **não** seguia — o antigo `PlayUnlockEffect` empilhava rotinas a cada Enter. `JunctionTotemsController.HandleUnblocked` só desativa o totem se ele **não** estiver sumindo; o `SetActive(false)` final é a última linha da própria `VanishRoutine`.

  > **Sumir com direção lê melhor que encolher no lugar** — era exatamente a queixa sobre o efeito antigo. O totem afunda `vanishSinkDepth` unidades com um giro leve, em vez de desaparecer no próprio eixo.

- **`UI/ChooseWayUI/ChooseWayScreenUI.cs`** — `PlayUnlocked(cost)` ativa o carimbo `UnlockedStamp`, zera o custo exibido, dispara o `-{custo}` flutuante e chama o punch. `Hide()` deixou de ser `SetActive(false)` seco: passa pelo `PlayExit` do animator e só então executa `HideImmediate`.

- **`UI/ChooseWayUI/ChooseWayScreenAnimator.cs`** — ganhou `PlayUnlockPunch()` e `PlayExit(onDone)`. Todas as rotinas agora zeram `_routine` ao terminar e fixam `anchoredPosition`/`localScale` na primeira linha — o CLAUDE.md já registrava que `PlayEnter` e `PlayPop` compartilham o mesmo slot de corrotina e que isso deixou a barra parada fora da tela uma vez.

- **`UI/ChooseWayUI/ChooseWayTotemBadge.cs`** — `PlayUnlocked()` (flash branco no anel e no ícone) e `PlayDismiss(duration)` (sobe `dismissRise` e faz fade pelo `CanvasGroup`).

  > **O punch do selo mora no animator, não no selo.** `ChooseWayBadgeAnimator.LateUpdate` **reescreve `plate.localScale` todo frame** — qualquer escala aplicada de fora por corrotina seria apagada no mesmo frame. Por isso o pop virou `ChooseWayBadgeAnimator.PlayPunch()`, um multiplicador temporário somado ao `_currentScale` dentro do próprio `LateUpdate`. Vale como regra: **não animar por fora uma propriedade que outro componente escreve continuamente.**

  > **Não há cadeado para animar.** `lock_`/`lockGlyph` continuam como campos do componente, mas o layout atual do selo não os liga mais (removidos em 27/08). Seguem null-safe e sem uso.

**Peças de cena a ligar (a UI é montada à mão, ver seção 3):** `unlockedStamp` (TMP sobre a `InfoColumn`), `costBurn` (TMP sobre a `CostColumn`) — ambos com `LayoutElement.ignoreLayout = true`, porque a soma das larguras das colunas da barra é um orçamento fechado — e um `CanvasGroup` no root do selo. **Todos os três são opcionais em runtime**: sem eles a sequência roda igual, só sem o carimbo, sem o `-{custo}` e sem o fade do selo.

## 5. Fluxo integrado (resumo)

1. `EnemySpawner` dispara `OnWaveStarted`/`OnWaveCleared` → `EventOrchestrator` decide (roleta, um evento por vez) se spawna baú ou totem de horda, conforme o `EventTiming` de cada tipo.
2. **Baú**: interação → raridade (`RarityRoller`, influenciado por sorte) → item (`ChestLootRoller`) → Take/Exile/Skip → `PlayerItemHandler.AcquireItem`.
3. **Horda**: aceite no totem → `HordeSpawner` gera pool ponderado, spawna em lotes, aplica multiplicador de dano global, recompensa por kill + bônus condicionado à sobrevivência.
4. **Loja de compra**: `ShopZone` (E) → estoque renovado a cada 3min ou sob demanda, sorteado por raridade+sorte, compra tudo-ou-nada debitando `PlayerStatsAggregator.Coins`. **Loja de venda** (local separado no mapa): `SellZone` (E) → jogador seleciona itens do próprio inventário, vende tudo-ou-nada por `SellManager.TrySellMultiple`, recebendo `preço de compra × (1 - sellDiscountPercent)` por item.
5. Ao limpar uma wave → `AbilityOrb` → 3 cartas (skill/arma nova/upgrade) via `AbilityDrawer`/`AbilitySelectionUI` → próxima wave liberada.
6. Moedas de waves/hordas/itens alimentam loja e desbloqueio de splines.

## 6. Lacunas conhecidas (design vs. implementado)

Itens da visão do jogo (seção 1) que **ainda não existem no código**:

- **Sem `GameManager` central** — nenhuma classe orquestra estado global, transições de cena ou game over. **Pendência concreta ligada a isso (25/08):** `PlayerSkillHandler.ResetForNewRun()`, `PlayerCartWeaponHandler.ResetForNewRun()` e `PlayerItemHandler.ResetForNewRun()` existem e funcionam, mas **ninguém os chama**. Hoje isso não causa bug porque o progresso vive em runtime e morre com o GameObject (ver 4.12); passa a causar no momento em que existir uma segunda run sem recarregar a cena (morrer → recomeçar). Quem criar o `GameManager` deve chamar os três.
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
