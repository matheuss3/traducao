# SCRIPT AUXILIAR PARA TRADUÇÃO DOS SISTEMAS MITITS
> Esse repositório serve para auxiliar na tradução dos termos dos sistemas da mitis para que sejam
devidamente cadastrados no banco de dados de termos e fiquem disponíveis para a tradução nos sistemas.

<br>

Esse script ainda está em desenvolvimento então fique a vontade para colaborar e incrementar mais funções
caso precise ou melhorias gerais.

<br><br>

## 0. Funcionalidades
>1 - Exportar Termos do Banco<br>
>2 - Criar query para inserir novos termos<br>
>3 - Enriquecer base (tradução inglês)<br>
>4 - Gerar lista com termos duplicados<br>
>5 - Gerar query de backup de termos<br>
>6 - Limpa arquivo com as mensagens das MessageBox<br>
>0 - Sair<br>

<br><br>

### 1 - Exportar Termos do Banco
Essa função permite que exporte os termos do banco de dados para um arquivo em excel caso deseje e tenha mais facilidade
em manipular as informações pelo excel, ou queira fazer alguma análise mais específica.<br><br>
> *Para conexão com o banco de dados crie um arquvivo **config.ini** com a seção **DATABASE** e a **URL_STRING** de conexão*

### 2 - Criar query para inserir novos termos
Essa função cria uma query baseado no arquivo "terms.xlsx" que deve está presente na pasta files/imports (ele já traduz dinâmicamente para o espanhol, antes da inserção). Verifique o layout
do arquivo nas observações abaixo.<br>

### 3 - Enriquece base (tradução inglês)
Altera o arquivo **"terms.xlsx"** da pasta files/imports inserindo na coluna TermIngles os termos da coluna Term que estão 
em português.<br><br>
> *Ele ajuda com a tradução de vários termos, porque a faz de forma automática, porém é necessário uma revisão da tradução ao final pois os alguns termos fora de contexto não são traduzidos corretamente pela api do google, principalmente termos abreviados.*

### 4 - Gera lista com termos duplicados
Gera uma lista com os termos duplicados no banco de dados, para evitar redundacia dos dados.<br><br>
> *Os termos tem como chave primária as colunas **Term** e **TermTypeAccept** então pode ser que existem dois termos iguais mas com o **TermTypeAccept** diferentes. Então não estranhe isso somente que um termo pode ser mais específico que outro ou que são aceitos em lugares específicos.*<br>
> *Utiliza o ultimo arquivo exportado do banco (Opção 1) para o backup então antes de executar aqui certifique-se que o arquvo esteja atualizado da forma que queira*

### 5 - Gera query de backup de termos
Cria uma query de **INSERT** com todos os termos cadastrados na tabela de termos.
> *Utiliza o ultimo arquivo exportado do banco (Opção 1) para o backup então antes de executar aqui certifique-se que o arquvo esteja atualizado da forma que queira*

### 6 - Limpa arquivo com as mensagens da MessageBox
Carregando... Aguarde!

<br>
<br>

## 1. Casos de Uso
### Caso 1 - Quero traduzir uma página do ERP
> **Etapas**
1. Coloque na pasta files/imports o arquivo *terms.xlsx* com os termos da página que deseja traduzir. Na pasta others possui a classe **FormExtractor** que pode ser adicionada ao projeto do ERP para auxiliar no mapeamento dos termos.<br>
2. Execute o script (pode utilizar o command.bat) e selecione a opção 3 para completar seu arquivo *terms.xlsx* com os termos em inglês.<br>
3. **REVISE OS TERMOS TRADUZIDOS**<br>
4. Execute a opção 2 que vai te perguntar se os termos são para o CRM, ou não. Neste caso selecione não digitando "n" (ou qualquer coisa diferente de "s"). Ele irá criar a query de insert na pasta oututs/inserts *ela vem com o timestamp de quando foi criada no nome*.
5. Execute a query criada.
6. Teste a função de traduzir formulário no ERP e seja feliz.
<br>

## 2. Observações
1. Caso utilize a classe **FormExtractor** para mapear todos os termos, cajo ainda falte termos ara traduzir repita o processo inserindo os termos faltantes manualmente no arquivo.
2. Também na pasta others possuem exemplos e templates dos arquivos que o script pode gerar, exemplos: terms.xlsx, arquivo de exportação, query de insert e arquivo json com os ids dos termos duplicados
3. A principal diferença entre o arquvo terms da pasta files/imports e files/exports é que o arquivo é que o arquivo que foi exportado pela função da opção **1** tem o timestamp de quando foi exportado
4. Lembre-se de configurar o local onde quer salvar o csv no arquivo  **FormExtractor.vb**

<br>
<br>

## 3. Requisitos
- python 3
- libs > googletrans, pandas, sqlalchemy, re, openpyxl (instale todas de uma vez com o comando abaixo)
```console
$ pip install -r requirements.txt
```