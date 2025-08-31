
# Desafio Umbler

Esta é uma aplicação web que recebe um domínio e mostra suas informações de DNS.

Este é um exemplo real de sistema que utilizamos na Umbler.

Ex: Consultar os dados de registro do dominio `umbler.com`

**Retorno:**
- Name servers (ns254.umbler.com)
- IP do registro A (177.55.66.99)
- Empresa que está hospedado (Umbler)

Essas informações são descobertas através de consultas nos servidores DNS e de WHOIS.

*Obs: WHOIS (pronuncia-se "ruís") é um protocolo específico para consultar informações de contato e DNS de domínios na internet.*

Nesta aplicação, os dados obtidos são salvos em um banco de dados, evitando uma segunda consulta desnecessaria, caso seu TTL ainda não tenha expirado.

*Obs: O TTL é um valor em um registro DNS que determina o número de segundos antes que alterações subsequentes no registro sejam efetuadas. Ou seja, usamos este valor para determinar quando uma informação está velha e deve ser renovada.*

Tecnologias Backend utilizadas:

- C#
- Asp.Net Core
- MySQL
- Entity Framework

Tecnologias Frontend utilizadas:

- Webpack
- Babel
- ES7

Para rodar o projeto você vai precisar instalar:

- dotnet Core SDK (https://www.microsoft.com/net/download/windows dotnet Core 6.0.201 SDK)
- Um editor de código, acoselhamos o Visual Studio ou VisualStudio Code. (https://code.visualstudio.com/)
- NodeJs v17.6.0 para "buildar" o FrontEnd (https://nodejs.org/en/)
- Um banco de dados MySQL (vc pode rodar localmente ou criar um site PHP gratuitamente no app da Umbler https://app.umbler.com/ que lhe oferece o banco Mysql adicionamente)

Com as ferramentas devidamente instaladas, basta executar os seguintes comandos:

Para "buildar" o javascript basta executar:

`npm install`
`npm run build`

Para Rodar o projeto:

Execute a migration no banco mysql:

`dotnet tool update --global dotnet-ef`
`dotnet tool ef database update`

E após: 

`dotnet run` (ou clique em "play" no editor do vscode)

# Objetivos:

Se você rodar o projeto e testar um domínio, verá que ele já está funcionando. Porém, queremos melhorar varios pontos deste projeto:

# FrontEnd

 - Os dados retornados não estão formatados, e devem ser apresentados de uma forma legível.
 - Não há validação no frontend permitindo que seja submetido uma requsição inválida para o servidor (por exemplo, um domínio sem extensão).
 - Está sendo utilizado "vanilla-js" para fazer a requisição para o backend, apesar de já estar configurado o webpack. O ideal seria utilizar algum framework mais moderno como ReactJs ou Blazor.  

# BackEnd

 - Não há validação no backend permitindo que uma requisição inválida prossiga, o que ocasiona exceptions (erro 500).
 - A complexidade ciclomática do controller está muito alta, o ideal seria utilizar uma arquitetura em camadas.
 - O DomainController está retornando a própria entidade de domínio por JSON, o que faz com que propriedades como Id, Ttl e UpdatedAt sejam mandadas para o cliente web desnecessariamente. Retornar uma ViewModel (DTO) neste caso seria mais aconselhado.

# Testes

 - A cobertura de testes unitários está muito baixa, e o DomainController está impossível de ser testado pois não há como "mockar" a infraestrutura.
 - O Banco de dados já está sendo "mockado" graças ao InMemoryDataBase do EntityFramework, mas as consultas ao Whois e Dns não. 

# Dica

- Este teste não tem "pegadinha", é algo pensado para ser simples. Aconselhamos a ler o código, e inclusive algumas dicas textuais deixadas nos testes unitários. 
- Há um teste unitário que está comentado, que obrigatoriamente tem que passar.
- Diferencial: criar mais testes.

# Entrega

- Enviei o link do seu repositório com o código atualizado.
- O repositório deve estar público para que possamos acessar..
- Modifique Este readme adicionando informações sobre os motivos das mudanças realizadas.

# Modificações:

- Minhas alterações foram mais focadas para o backend da aplicação, devido a minha experiência na área. Meu foco foi trazer melhorias de estrutura, observabilidade e código limpo em geral. Segue minhas mudanças:

## Controllers

 - Como vi que o controller da aplicação estava fazendo muita coisa e tinha muitas funções, eu optei por separar os endpoints em camadas, utilizando injeção de dependência para criar um serviço responsável por gerenciar toda a lógica do domínio, desse jeito fica mais facil criar testes, controlar o que vai ser retornado da API e deixa o processo mais fácil de entender
 - Criei uma classe pra retornar pro frontend só os dados necessários do domínio, separando assim a tabela do banco do que de fato é retornado pelo frontend
 - Coloquei alguns métodos para deixar padrão o retorno da API, seja ele um retorno com erros ou um retorno bem sucedido, dessa forma fica mais facil para o front manipular o JSON retornado. Eu alterei para que a lógica seja sempre retornada dentro de uma classe de retorno, com mensagens que podem ser mostradas ao usuário.
 - Criei um atributo chamado DomainOrIpAttribute, que é responsável por validar se o que o usuário digitou no front é um domínio válido ou um ip válido, desse jeito ele não chega no serviço com dados inválidos.
 - Dei uma corrigida no nome da rota para deixar no padrão RESTful

## Models 

 - Organizei a estrutura de pastas das models, separando em responsabilidaedes (retorno da api, modelo pro front etc)
 - Eu usei um container docker pra rodar o mysql na minha maquina, por que não consegui usar a connection string que tinha no repositório


## Persistence

 - Guardei aqui tudo relacionado a acesso a banco de dados, para deixar mais facil de entender a arquitetura do projeto

## Services

 - Aqui eu deixei tanto os serviços que vão ser chamados pelas controllers quanto os serviços que são usados para acessos fora da API (o WhoIsService). Eu criei um DomainsService para ele armazenar toda a lógica de buscar, cadastrar e alterar domínios cadastrados na base de dados, consultar no serviço do WhoIs etc
 - Criei e injetei serviços para acessar as classes de LookupClient e WhoIs, para que eu possa mockar eles nos testes unitários do DomainsService
 - Adicionei um ILogger para logar qualquer erro que aconteça na execução do método, para que esses erros possam ser consultados depois em um arquivo, DB ou algo assim
 - Refatorei a lógica de consulta do domínio e isolei numa função "GetDomainDetails", para que eu possa usar ela várias vezes sem duplicar código, e também para deixa-lo mais fácil de entender
 - Tinha um bug ao obter o ARecords do LookupClient que fazia ele não retornar nenhum item, eu corrigi isso

## Tests

 - Adicionei testes unitários para os serviços criados e pro controller do Domínio

# FrontEnd

- Na parte do frontend possui validação de IP para complementar. Eu fiz isso pois pareceu que precisava disso, porém o whoIsService não funciona com ele pelos testes que eu realizei 
- Utilizei blazor para chamar o serviço de domínio e também para isolar componentes da aplicação como a consulta de domínio, deixando a aplicação mais moderna
- Quando pesquisar um domínio, enquanto realiza a pesquisa o usuário não pode atualizar a resposta
- O retorno das informações do domínio foram colocados em uma tabela
- Adicionei uma função para pesquisar o domínio ao apertar o enter
- Eu arrumei levemente o layout da página, quis preservar o layout original da página
- Mudei a versão webpack da versão 3.8 para a 5, pois não consegui fazer funcionar sem fazer essa modificação
- Adicionei validações para o usuário não poder enviar qualquer coisa na resposta que não seja um domínio ou ip válido com extensão
- Preservei o endpoint caso ele seja utilizado por outras aplicações (app mobile por exemplo)
- Tentei dar meu melhor na parte do frontend porém meu ponto forte é mais na área do backend. Eu continuo estudando e me dedicando pois gosto muito de frontend também