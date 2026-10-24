/*
   Copyright 2016-2022 LIPtoH <liptoh.codebase@gmail.com>

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using TS_SE_Tool.Utilities;
using System.Configuration;

namespace TS_SE_Tool
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Headless diagnostic mode - see CustomClasses\Diagnostics\SelfTest.cs
            string[] cmdArgs = Environment.GetCommandLineArgs().Skip(1).ToArray();

            if (cmdArgs.Length > 0 && cmdArgs[0] == "--selftest")
            {
                Environment.ExitCode = Diagnostics.SelfTest.Run(cmdArgs);
                return;
            }

            // Add the event handler for handling UI thread exceptions to the event
            Application.ThreadException += new ThreadExceptionEventHandler(UIThreadException);

            // Set the unhandled exception mode to force all Windows Forms errors to go through handler
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

            // Add the event handler for handling non-UI thread exceptions to the event
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            IO_Utilities.LogWriter("--- START ---");
            IO_Utilities.LogWriter(AssemblyData.AssemblyProduct + " - " + AssemblyData.AssemblyVersion);
            DetectEnviroment.DetectOS();
            DetectEnviroment.Get45PlusFromRegistry();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FormMain());
            IO_Utilities.LogWriter("--- END ---");
        }

        // Handle the UI exceptions by showing a dialog box, and asking the user whether
        // or not they wish to abort execution.
        private static void UIThreadException(object sender, ThreadExceptionEventArgs t)
        {
            DialogResult result = DialogResult.Cancel;
            try
            {
                Exception ex = t.Exception;

                string errorMsg = "An application error occurred. Please contact the Developer at " + Utilities.Web_Utilities.External.linkMailDeveloper + " . Information can be found in \" Errorlog \" file.";

                IO_Utilities.ErrorLogWriter(ex.Message + "\n\nStack Trace:\n" + ex.StackTrace);

                result = TS_SE_Tool.LocalizedMessageBox.Show(errorMsg, "Windows Forms Error", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Stop);
            }
            catch
            {
                try
                {
                    TS_SE_Tool.LocalizedMessageBox.Show("Fatal Windows Forms Error", "Fatal Windows Forms Error", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Stop);
                }
                finally
                {
                    Application.Exit();
                }
            }

            // Exits the program when the user clicks Abort.
            if (result == DialogResult.Abort)
                Application.Exit();
        }

        // Handle the UI exceptions by showing a dialog box, and asking the user whether
        // or not they wish to abort execution.
        // NOTE: This exception cannot be kept from terminating the application - it can only 
        // log the event, and inform the user about it. 
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                Exception ex = (Exception)e.ExceptionObject;

                string errorMsg = "An application error occurred. Please contact the Developer at " + Utilities.Web_Utilities.External.linkMailDeveloper + " . Information can be found in \" Errorlog \" file.";

                TS_SE_Tool.LocalizedMessageBox.Show(errorMsg, "Non-UI Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                IO_Utilities.ErrorLogWriter(ex.Message + "\n\nStack Trace:\n" + ex.StackTrace);
            }
            catch (Exception exc)
            {
                try
                {
                    TS_SE_Tool.LocalizedMessageBox.Show("Fatal Non-UI Error. Could not write the error to the event log.\r\nReason: " 
                        + exc.Message, "Fatal Non-UI Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                }
                finally
                {
                    Application.Exit();
                }
            }
        }
    }

    internal static class LocalizedMessageBox
    {
        private static readonly Dictionary<string, string> Portuguese = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            { "You have unsaved changes.", "Há alterações não salvas." },
            { "Do you really want to close down application?", "Deseja realmente fechar o aplicativo?" },
            { "Close Application", "Fechar aplicativo" },
            { "Close Application without saving changes", "Fechar aplicativo sem salvar as alterações" },
            { "An application error occurred. Please contact the Developer at ", "Ocorreu um erro no aplicativo. Entre em contato com o desenvolvedor pelo e-mail " },
            { " . Information can be found in \" Errorlog \" file.", ". Consulte o arquivo errorlog.log para obter detalhes." },
            { "Windows Forms Error", "Erro do Windows Forms" },
            { "Non-UI Error", "Erro fora da interface" },
            { "Fatal Windows Forms Error", "Erro fatal do Windows Forms" },
            { "Fatal Non-UI Error", "Erro fatal fora da interface" },
            { "Fatal Non-UI Error. Could not write to the event log.\r\nReason: ", "Erro fatal fora da interface. Não foi possível registrar o erro.\r\nMotivo: " },
            { "Some of the blocks in save file was not recognized and it may affect Program behavior.", "Alguns blocos do arquivo de save não foram reconhecidos e podem afetar o funcionamento do programa." },
            { "File saved", "Arquivo salvo" },
            { "Saving", "Salvando" },
            { "Savefile not saved.", "O arquivo de save não foi gravado." },
            { "Something gone wrong with decoding.", "Ocorreu um erro ao decodificar o arquivo." },
            { "Something gone wrong.", "Ocorreu um erro." },
            { "Wrongly decoded Save file or wrong file format", "O arquivo de save foi decodificado incorretamente ou está em formato inválido." },
            { "Could not decrypt after 5 attempts", "Não foi possível descriptografar após 5 tentativas." },
            { "The saved update-check setting could not be read. You can continue; update checking is disabled for this launch.", "Não foi possível ler a configuração de busca por atualizações. A busca ficará desativada nesta execução." },
            { "This will open ", "Isto abrirá " },
            { " web-page.", " no navegador." },
            { "Do you want to continue?", "Deseja continuar?" },
            { "Path ", "O caminho " },
            { " already added to the ", " já foi adicionado à lista de " },
            { " added to the ", " foi adicionado à lista de " },
            { " list", "" },
            { "Path exist in the list", "Caminho já adicionado" },
            { "Company name is empty.", "O nome da empresa está vazio." },
            { "Company name", "Nome da empresa" },
            { "Attention! Loosing content", "Atenção! O conteúdo será perdido" },
            { "Restoring from backup file will overwrite existing file.", "Restaurar o backup substituirá o arquivo existente." },
            { "Restoring from backup file will overwrite existing save file.", "Restaurar o backup substituirá o save existente." },
            { "Restoring Profile from Backup", "Restaurar perfil do backup" },
            { "Restoring Save file from Backup", "Restaurar save do backup" },
            { "Select: Yes - Overwrite | No - Swap files | Cancel - Abort restoring.", "Selecione: Sim — substituir | Não — trocar arquivos | Cancelar — cancelar a restauração." },
            { "Conflicting save file names", "Nomes de arquivos de save conflitantes" },
            { "Non unique save file names", "Os nomes dos arquivos de save não são exclusivos" },
            { "You importing saves with non unique names.\r\nThis can lead to confusion. Do you want to continue?\r\nYes - Continue. No - Cancel.", "Você está importando saves com nomes repetidos.\r\nIsso pode causar confusão. Deseja continuar?\r\nSim — continuar. Não — cancelar." },
            { "Some save files already exist.\r\nDo you want to overwrite save files?\r\nYes - overwrite. No - create saves with same names. Cancel - change selected saves.", "Alguns saves já existem.\r\nDeseja substituí-los?\r\nSim — substituir. Não — criar saves com os mesmos nomes. Cancelar — alterar a seleção." },
            { "Do you want to discard changes and switch game type?", "Deseja descartar as alterações e trocar de jogo?" },
            { "Do you want to save Drivers and Trucks from sold garages?", "Deseja salvar os motoristas e caminhões das garagens vendidas?" },
            { "Files was exported.", "Os arquivos foram exportados." },
            { "Files was imported.", "Os arquivos foram importados." },
            { "Export", "Exportar" },
            { "Import", "Importar" },
            { "No valid profiles found", "Nenhum perfil válido foi encontrado." },
            { "No valid items", "Nenhum item válido foi encontrado." },
            { "Non valid item", "Item inválido" },
            { "No profiles created due to duplicating names.", "Nenhum perfil foi criado porque os nomes estão duplicados." },
            { "New Profiles created ( ", "Novos perfis criados ( " },
            { "New Profile name - ", "Nome do novo perfil — " },
            { "Profile name is not applied", "O nome do perfil não foi aplicado." },
            { "Profile write failed", "Falha ao gravar o perfil." },
            { "Profile not decoded", "Não foi possível decodificar o perfil." },
            { "Profile has wrong version/format", "O perfil tem versão ou formato inválido." },
            { "Profile properties was not applied", "As propriedades do perfil não foram aplicadas." },
            { "Profile backup creation failed. Keeping both versions. Delete old profile manually.", "Falha ao criar o backup do perfil. As duas versões foram mantidas. Exclua o perfil antigo manualmente." },
            { "Deleting old profile failed. Keeping both versions. Delete old profile manually.", "Falha ao excluir o perfil antigo. As duas versões foram mantidas. Exclua o perfil antigo manualmente." },
            { "Deleting new Profile.", "Excluindo o novo perfil." },
            { "Unexpected error. Deleting new Profile.", "Erro inesperado. Excluindo o novo perfil." },
            { "Create new folder name failed", "Falha ao criar o nome da nova pasta." },
            { "Directory was not created", "A pasta não foi criada." },
            { "Directory copy failed", "Falha ao copiar a pasta." },
            { "Directory for saves was not created", "A pasta dos saves não foi criada." },
            { "Directory with saves copy failed", "Falha ao copiar os saves." },
            { "Album Directory copy failed", "Falha ao copiar a pasta do álbum." },
            { "Error during save preview fixing", "Erro ao corrigir a prévia do save." },
            { "File does not exist in ", "O arquivo não existe em " },
            { "File not exist", "Arquivo não encontrado" },
            { "Could not read: ", "Não foi possível ler: " },
            { "Download failed", "Falha no download" },
            { "Unable to download new version. Try later or download new version manually.", "Não foi possível baixar a nova versão. Tente novamente mais tarde ou faça o download manualmente." },
            { "Unable to find Updater.exe.", "Não foi possível encontrar o Updater.exe." },
            { "Unable to find Updater.exe. Please update manually. New version located in Updater folder.", "Não foi possível encontrar o Updater.exe. Atualize manualmente; a nova versão está na pasta Updater." },
            { "New version located in Updater folder.", "A nova versão está na pasta Updater." },
            { "Updater not exist. Manual update", "O atualizador não foi encontrado. Faça a atualização manualmente." },
            { "Please update manually.", "Atualize manualmente." },
            { "Missing manual. Try to repair via update", "Manual ausente. Tente reparar usando a atualização." },
            { "HowTo.pdf not found", "HowTo.pdf não foi encontrado." },
            { "Standart Save folders does not exist for this game - ", "As pastas padrão de saves não existem para este jogo — " },
            { "Check game installation, Start game and Refresh profiles list or Add Custom paths.", "Verifique a instalação do jogo, inicie-o e atualize a lista de perfis ou adicione caminhos personalizados." },
            { "Please select another folder", "Selecione outra pasta." },
            { "Please select existing save file not marked for deleting.", "Selecione um save existente que não esteja marcado para exclusão." },
            { "Cannot display the image: ", "Não foi possível exibir a imagem: " },
            { "Please select Base save file for New saves.", "Selecione o save base para os novos saves." },
            { "Wrong data. Expected GPS Path data but", "Dados inválidos. Esperava dados de caminho GPS, mas encontrou" },
            { "Wrong data. Expected Paint data but", "Dados inválidos. Esperava dados de pintura, mas encontrou" },
            { "Wrong data. Expected Position data but", "Dados inválidos. Esperava dados de posição, mas encontrou" },
            { "Wrong data. Expected Truck GPS data but", "Dados inválidos. Esperava dados de GPS do caminhão, mas encontrou" },
            { "Expected Color data but", "Esperava dados de cor, mas encontrou" },
            { "Color data has been copied", "Os dados de cor foram copiados." },
            { "Color data has been inserted", "Os dados de cor foram inseridos." },
            { "GPS Path data has been copied.", "Os dados de caminho GPS foram copiados." },
            { "GPS Path data has been inserted.", "Os dados de caminho GPS foram inseridos." },
            { "Truck GPS position has been copied.", "A posição GPS do caminhão foi copiada." },
            { "Truck GPS position has been inserted.", "A posição GPS do caminhão foi inserida." },
            { "Paint data  has been inserted.", "Os dados de pintura foram inseridos." },
            { "Paint data has been copied.", "Os dados de pintura foram copiados." },
            { "Position data has been inserted.", "Os dados de posição foram inseridos." },
            { "Positions has been copied.", "As posições foram copiadas." },
            { "Hue must be a number value between 0 and 360", "Matiz deve ser um número entre 0 e 360." },
            { "Saturation must be a number value between 0 and 100", "Saturação deve ser um número entre 0 e 100." },
            { "Brightness must be a number value between 0 and 100.", "Brilho deve ser um número entre 0 e 100." },
            { "Red must be a number value between 0 and 255", "Vermelho deve ser um número entre 0 e 255." },
            { "Green must be a number value between 0 and 255", "Verde deve ser um número entre 0 e 255." },
            { "Blue must be a number value between 0 and 255", "Azul deve ser um número entre 0 e 255." },
            { "Cyan must be a number value between 0 and 100", "Ciano deve ser um número entre 0 e 100." },
            { "Magenta must be a number value between 0 and 100", "Magenta deve ser um número entre 0 e 100." },
            { "Yellow must be a number value between 0 and 100", "Amarelo deve ser um número entre 0 e 100." },
            { "Key must be a number value between 0 and 100", "Preto deve ser um número entre 0 e 100." },
            { "Hex must be a hex number between 0x000000 and 0xFFFFFF", "O valor hexadecimal deve estar entre 0x000000 e 0xFFFFFF." },
            { "An integer between 0 and 360 is required.\nClosest value inserted.", "É necessário um número inteiro entre 0 e 360.\nO valor inteiro mais próximo foi inserido." },
            { "An integer between 0 and 100 is required.\nClosest value inserted.", "É necessário um número inteiro entre 0 e 100.\nO valor inteiro mais próximo foi inserido." },
            { "An integer between 0 and 255 is required.\nClosest value inserted.", "É necessário um número inteiro entre 0 e 255.\nO valor inteiro mais próximo foi inserido." },
            { "Do you know the answer to life the universe and everything?", "Você sabe a resposta para a vida, o universo e tudo mais?" },
            { "ZIP Archive Created.\r\nOpen destination folder?", "Arquivo ZIP criado.\r\nDeseja abrir a pasta de destino?" },
            { "Success", "Sucesso" },
            { "Error", "Erro" },
            { "Exception.", "Exceção." },
            { "SQL Exception.", "Exceção SQL." },
            { "SQL Exception. Update DB", "Exceção SQL — atualizar banco de dados" },
            { "SQL Exception. Create DB", "Exceção SQL — criar banco de dados" },
            { "SQL Exception. Load all Distances", "Exceção SQL — carregar distâncias" },
            { "SQL Exception. Ext Data", "Exceção SQL — dados externos" },
            { "Support developer", "Apoiar o desenvolvedor" },
            { "Switching game", "Trocando de jogo" },
            { "Settings warning", "Aviso de configuração" },
            { "Information can be found in error.log file.", "Consulte o arquivo errorlog.log para obter detalhes." },
            { "it may be corrupt.\r\n\r\nError: ", "o arquivo pode estar corrompido.\r\n\r\nErro: " },
            { "was found.", "foi encontrado." },
            { "web-page.", "página na web." },
            { "Error. Wrong data.", "Erro. Dados inválidos." },
            { "Custom path", "Caminho personalizado" },
            { "Deleting item", "Excluindo item" },
            { "Dialogue close", "Fechar janela" },
            { "Hex must be a hex value between 0x000000 and 0xFFFFFF", "O valor hexadecimal deve estar entre 0x000000 e 0xFFFFFF." },
            { "It must contain at least 1 letter.", "É necessário conter ao menos uma letra." },
            { "Not a valid save file for base file.", "O arquivo não é um save válido para o arquivo base." },
            { "Please contact Developer via e-mail <", "Entre em contato com o desenvolvedor pelo e-mail <" },
            { "Saves are created.", "Os saves foram criados." },
            { "Now you can close this window.", "Agora você pode fechar esta janela." },
            { "Unidentified blocks in save file", "Blocos não identificados no arquivo de save" },
            { "Some save files already exist.", "Alguns arquivos de save já existem." },
            { "Do you want to overwrite save files?", "Deseja substituir os arquivos de save?" },
            { "Yes - overwrite. No - create saves with same names. Cancel - change selected saves.", "Sim — substituir. Não — criar saves com os mesmos nomes. Cancelar — alterar a seleção." },
            { "You importing saves with non unique names.", "Você está importando saves com nomes repetidos." },
            { "This can lead to confusion. Do you want to continue?", "Isso pode causar confusão. Deseja continuar?" },
            { "Yes - Continue. No - Cancel.", "Sim — continuar. Não — cancelar." },
            { "ZIP Archive Created.", "Arquivo ZIP criado." },
            { "Open destination folder?", "Deseja abrir a pasta de destino?" },
            { "Do you really want to close dialogue without saving?", "Deseja fechar a janela sem salvar?" },
            { "Just a text", "Apenas um texto" },
            { "A caption", "Um título" },
            { "Some caption", "Um título" },
            { "Some text with a link: www.google.com", "Um texto com link: www.google.com" },
            { "A text", "Um texto" },
            { "The save could not be loaded. Details were written to errorlog.log.", "Não foi possível carregar o save. Os detalhes foram registrados em errorlog.log." },
            { "Save load failed", "Falha ao carregar o save" }
        };

        internal static DialogResult Show(params object[] arguments)
        {
            if (arguments == null || arguments.Length == 0)
                return DialogResult.None;

            int index = 0;
            IWin32Window owner = arguments[0] as IWin32Window;
            if (owner != null)
                index++;

            int suppliedCount = arguments.Length - index;
            string text = arguments[index] as string ?? Convert.ToString(arguments[index]);
            text = Translate(text);
            index++;

            string caption = String.Empty;
            if (index < arguments.Length && arguments[index] is string)
                caption = Translate((string)arguments[index++]);

            MessageBoxButtons buttons = MessageBoxButtons.OK;
            MessageBoxIcon icon = MessageBoxIcon.None;
            MessageBoxDefaultButton defaultButton = MessageBoxDefaultButton.Button1;
            System.Windows.Forms.SizeGripStyle gripStyle = System.Windows.Forms.SizeGripStyle.Hide;

            if (index < arguments.Length && arguments[index] is MessageBoxButtons) buttons = (MessageBoxButtons)arguments[index++];
            if (index < arguments.Length && arguments[index] is MessageBoxIcon) icon = (MessageBoxIcon)arguments[index++];
            if (index < arguments.Length && arguments[index] is MessageBoxDefaultButton) defaultButton = (MessageBoxDefaultButton)arguments[index++];
            if (index < arguments.Length && arguments[index] is System.Windows.Forms.SizeGripStyle) gripStyle = (System.Windows.Forms.SizeGripStyle)arguments[index++];

            if (owner == null)
            {
                switch (suppliedCount)
                {
                    case 1: return JR.Utils.GUI.Forms.FlexibleMessageBox.Show(text);
                    case 2: return JR.Utils.GUI.Forms.FlexibleMessageBox.Show(text, caption);
                    case 3: return JR.Utils.GUI.Forms.FlexibleMessageBox.Show(text, caption, buttons);
                    case 4: return JR.Utils.GUI.Forms.FlexibleMessageBox.Show(text, caption, buttons, icon);
                    case 5: return JR.Utils.GUI.Forms.FlexibleMessageBox.Show(text, caption, buttons, icon, defaultButton);
                    default: return JR.Utils.GUI.Forms.FlexibleMessageBox.Show(text, caption, buttons, icon, defaultButton, gripStyle);
                }
            }

            switch (suppliedCount)
            {
                case 2: return JR.Utils.GUI.Forms.FlexibleMessageBox.Show(owner, text);
                case 3: return JR.Utils.GUI.Forms.FlexibleMessageBox.Show(owner, text, caption);
                case 4: return JR.Utils.GUI.Forms.FlexibleMessageBox.Show(owner, text, caption, buttons);
                case 5: return JR.Utils.GUI.Forms.FlexibleMessageBox.Show(owner, text, caption, buttons, icon);
                case 6: return JR.Utils.GUI.Forms.FlexibleMessageBox.Show(owner, text, caption, buttons, icon, defaultButton);
                default: return JR.Utils.GUI.Forms.FlexibleMessageBox.Show(owner, text, caption, buttons, icon, defaultButton, gripStyle);
            }
        }

        private static readonly Dictionary<string, string> ResourceKeys = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            { "An application error occurred. Please contact the Developer at ", "messageBoxApplicationErrorPrefix" },
            { " . Information can be found in \" Errorlog \" file.", "messageBoxApplicationErrorSuffix" },
            { "Fatal Non-UI Error. Could not write to the event log.\r\nReason: ", "messageBoxFatalNonUiReasonPrefix" },
            { "Fatal Windows Forms Error", "messageBoxFatalWindowsFormsError" },
            { "Fatal Non-UI Error", "messageBoxFatalNonUiError" },
            { "Windows Forms Error", "messageBoxWindowsFormsError" },
            { "Non-UI Error", "messageBoxNonUiError" }
        };

        internal static string Translate(string value)
        {
            if (String.IsNullOrEmpty(value))
                return value;

            System.Globalization.CultureInfo culture = Thread.CurrentThread.CurrentUICulture;
            try
            {
                PlainTXTResourceManager resourceManager = new PlainTXTResourceManager();
                foreach (KeyValuePair<string, string> item in ResourceKeys.OrderByDescending(x => x.Key.Length))
                {
                    string localized = resourceManager.GetString(item.Value, culture);
                    if (!String.IsNullOrEmpty(localized) && localized != item.Key)
                        value = value.Replace(item.Key, localized);
                }
            }
            catch
            {
                // Keep the original message if localization resources are unavailable during startup.
            }

            if (!culture.Name.StartsWith("pt", StringComparison.OrdinalIgnoreCase))
                return value;

            string translated;
            if (Portuguese.TryGetValue(value, out translated))
                return translated;

            foreach (KeyValuePair<string, string> item in Portuguese.OrderByDescending(x => x.Key.Length))
                if (item.Key.Length >= 8 && value.IndexOf(item.Key, StringComparison.Ordinal) >= 0)
                    value = value.Replace(item.Key, item.Value);

            return value;
        }
    }}
