using System;

namespace TodoApp.Commands
{
    public class HelpCommand : ICommand
    {
        public void Execute()
        {
            Console.WriteLine(@"
ДОСТУПНЫЕ КОМАНДЫ
help                             - показать справку
profile [-o]                     - показать профиль / выйти из профиля
add ""текст""                      - добавить задачу
add -m/--multiline               - добавить задачу в многострочном режиме
view [-i] [-s] [-d] [-a]         - показать задачи
read <idx>                       - показать полный текст задачи
status <idx> <статус>            - изменить статус задачи
update <idx> ""новый текст""       - обновить текст задачи
delete <idx>                     - удалить задачу
search [параметры]               - найти задачи
load <count> <size>              - запустить имитацию загрузок
sync --push                      - отправить данные на сервер
sync --pull                      - получить данные с сервера
undo                             - отменить последнее действие
redo                             - повторить отменённое действие
exit                             - выход из программы
");
        }
    }
}
