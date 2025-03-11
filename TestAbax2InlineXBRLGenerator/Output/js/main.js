let locale = 'es';

let messages = {
    es: {
        roles: {
            role_105000_title: '[105000] Comentarios y Análisis de la Administración',
            role_110000_title: '[110000] Información general sobre estados financieros',
            role_210000_title: '[210000] Estado de situación financiera, circulante/no circulante',
            role_310000_title: '[310000] Estado de resultados, resultado del periodo, por función de gasto',
            role_410000_title: '[410000] Estado del resultado integral, componentes ORI presentados netos de impuestos',
            role_520000_title: '[520000] Estado de flujos de efectivo, método indirecto',
            role_610000_current_title: '[610000] Estado de cambios en el capital contable - Acumulado actual',
            role_610000_previous_title: '[610000] Estado de cambios en el capital contable - Acumulado anterior',
            role_700000_title: '[700000] Datos informativos del Estado de situación financiera',
            role_700002_title: '[700002] Datos informativos del estado de resultados',
            role_700003_title: '[700003] Datos informativos- Estado de resultados 12 meses',
            role_800001_title: '[800001] Anexo - Desglose de créditos',
            role_800003_title: '[800003] Anexo - Posición monetaria en moneda extranjera',
            role_800005_title: '[800005] Anexo - Distribución de ingresos por producto',
            role_800007_title: '[800007] Anexo - Instrumentos financieros derivados',
            role_800100_title: '[800100] Notas - Subclasificaciones de activos, pasivos y capital contable',
            role_800200_title: '[800200] Notas - Análisis de ingresos y gastos',
            role_800500_title: '[800500] Notas - Lista de notas',
            role_800600_title: '[800600] Notas - Lista de políticas contables',
            role_813000_title: '[813000] Notas - Información financiera intermedia de conformidad con la NIC 34',
        },
        languages: {
            es: 'Español',
            en: 'Inglés'
        },
        controls: {
            next: 'Sig',
            prev: 'Ant',
            exportTable: 'Exportar tabla',
            search: 'Buscar'
        },
        document: {
            quarterly_financial_information_title: 'Información financiera trimestral',
            cover_search_placeholder: 'Ingresa una pregunta, tema o palabra clave',
            faq_title: 'Preguntas más frecuentes',
            faq_intro: 'Aquí encontrarás las preguntas más frecuentes que puedes revisar antes de comenzar',
            search_placeholder: 'Buscar en el documento...',
            search_results: 'Resultados de la búsqueda:',
            no_results: 'No se encontraron resultados',
            go_to_page: 'Ir a la página',
            show_all_search_results: 'Mostrar todos los resultados de la búsqueda'
        }
    },
    en: {
        roles: {
            role_105000_title: '[105000] Management commentary',
            role_110000_title: '[110000] General information about financial statements',
            role_210000_title: '[210000] Statement of financial position, current/non-current',
            role_310000_title: '[310000] Statement of comprehensive income, profit or loss, by function of expense',
            role_410000_title: '[410000] Statement of comprehensive income, OCI components presented net of tax',
            role_520000_title: '[520000] Statement of cash flows, indirect method',
            role_610000_current_title: '[610000] Statement of changes in equity - Current year-to-date',
            role_610000_previous_title: '[610000] Statement of changes in equity - Previous year-to-date',
            role_700000_title: '[700000] Informative data about the Statement of financial position',
            role_700002_title: '[700002] Informative data about the Income statement',
            role_700003_title: '[700003] Informative data - Income statement for 12 months',
            role_800001_title: '[800001] Annex - Breakdown of credits',
            role_800003_title: '[800003] Annex - Monetary position in foreign currency',
            role_800005_title: '[800005] Annex - Distribution of income by product',
            role_800007_title: '[800007] Annex - Financial derivative instruments',
            role_800100_title: '[800100] Notes - Subclassifications of assets, liabilities and equities',
            role_800200_title: '[800200] Notes - Analysis of income and expenses',
            role_800500_title: '[800500] Notes - List of notes',
            role_800600_title: '[800600] Notes - List of accounting policies',
            role_813000_title: '[813000] Notes - Interim financial reporting',
        },
        languages: {
            es: 'Spanish',
            en: 'English'
        },
        controls: {
            next: 'Next',
            prev: 'Prev',
            exportTable: 'Export table',
            search: 'Search'
        },
        document: {
            quarterly_financial_information_title: 'Quarterly financial information',
            cover_search_placeholder: 'Enter a question, topic or keyword',
            faq_title: 'Most frequently asked questions',
            faq_intro: 'Here are the most frequently asked questions you may check before getting started',
            search_placeholder: 'Search in document...',
            search_results: 'Search results:',
            no_results: 'No results found',
            go_to_page: 'Go to page',
            show_all_search_results: 'Show all search results'
        }
    }
}

var reportStatus = {
    roles: [
        {
            id: '105000',
            labelKey: 'roles.role_105000_title',
            contentElement: null,
            collapsedResults: false
        },
        {
            id: '110000',
            labelKey: 'roles.role_110000_title',
            contentElement: null,
            collapsedResults: false
        },
        {
            id: '210000',
            labelKey: 'roles.role_210000_title',
            contentElement: null,
            collapsedResults: false
        },
        {
            id: '310000',
            labelKey: 'roles.role_310000_title',
            contentElement: null,
            collapsedResults: false
        },
        {
            id: '410000',
            labelKey: 'roles.role_410000_title',
            contentElement: null,
            collapsedResults: false
        },
        {
            id: '520000',
            labelKey: 'roles.role_520000_title',
            contentElement: null,
            collapsedResults: false
        },
        {
            id: '610000_current',
            labelKey: 'roles.role_610000_current_title',
            contentElement: null,
            collapsedResults: false
        },
        {
            id: '610000_previous',
            labelKey: 'roles.role_610000_previous_title',
            contentElement: null,
            collapsedResults: false
        },
        {
            id: '700000',
            labelKey: 'roles.role_700000_title',
            contentElement: null,
            collapsedResults: false
        },
        {
            id: '700002',
            labelKey: 'roles.role_700002_title',
            contentElement: null,
            collapsedResults: false
        },
        {
            id: '700003',
            labelKey: 'roles.role_700003_title',
            contentElement: null,
            collapsedResults: false
        },
        {
            id: '800001',
            labelKey: 'roles.role_800001_title',
            contentElement: null,
            collapsedResults: false
        },
        {
            id: '800003',
            labelKey: 'roles.role_800003_title',
            contentElement: null,
            collapsedResults: false
        },
        {
            id: '800005',
            labelKey: 'roles.role_800005_title',
            contentElement: null,
            collapsedResults: false
        },
        {
            id: '800007',
            labelKey: 'roles.role_800007_title',
            contentElement: null,
            collapsedResults: false
        },
        {
            id: '800100',
            labelKey: 'roles.role_800100_title',
            contentElement: null,
            collapsedResults: false
        },
        {
            id: '800200',
            labelKey: 'roles.role_800200_title',
            contentElement: null,
            collapsedResults: false
        },
        {
            id: '800500',
            labelKey: 'roles.role_800500_title',
            contentElement: null,
            collapsedResults: false
        },
        {
            id: '800600',
            labelKey: 'roles.role_800600_title',
            contentElement: null,
            collapsedResults: false
        },
        {
            id: '813000',
            labelKey: 'roles.role_813000_title',
            contentElement: null,
            collapsedResults: false
        }
    ],
    allResults: [],
    currentResult: '',
    currentPage: 1,
    resultsPerPage: 5,
    roleElements: [],
    searchTerm: null,
    selectedElement: null,
    languagesDropdownOpen: false,
    searchResultsDropdownOpen: false,
    showSearchResults: false,
    showCover: true,
    showSearch: false,
    darkMode: false,
    fullscreen: false,
    dropdownOpen: false,
    hideNavigationDrawer: false,
    navigationDrawerOpen: false,
    currentRole: '210000',
    currentRoleIndex: 2,
    isRoleVisible: function (role) {
        return this.currentRole === role;
    },
    setCurrentRole: function (role, highlightId) {
        this.currentRoleIndex = this.roles.findIndex((r) => r.id === role);
        this.currentRole = role;
        if (highlightId && highlightId !== undefined) {
            this.showSearchResults = true;
            this.$nextTick(() => {
                const highlightElement = document.getElementById(highlightId);
                if (highlightElement) {
                    highlightElement.scrollIntoView({ behavior: 'smooth', block: 'center' });

                    // Esperar un poco para asegurar que el scroll haya terminado
                    setTimeout(() => {
                        highlightElement.classList.add('animate-highlight');

                        // Remover la clase después de la animación para permitir futuras animaciones
                        highlightElement.addEventListener('animationend', () => {
                            highlightElement.classList.remove('animate-highlight');
                        }, { once: true });
                    }, 500); // Ajusta el tiempo si es necesario
                }
            });
            this.currentResult = highlightId;
        }
    },
    clearSearch() {
        this.currentResult = '';
        this.showSearchResults = false;
        this.searchTerm = null;
        this.allResults = [];
        this.roles.forEach(role => {
            if (role.contentElement !== null) {
                role.contentElement.innerHTML = role.contentElement.innerHTML.replace(/<span id="highlight-\d+" class="highlight">(.*?)<\/span>/g, '$1');
            }
        });
    },
    checkSearchPanelDropdownVisibility() {
        if (this.searchTerm !== null && this.searchTerm.length >= 3 && this.allResults && this.allResults.length > 0 && !this.showSearchResults) {
            this.searchResultsDropdownOpen = true;
            this.dropdownOpen = true;
        }
    },
    openSearchResults() {
        this.dropdownOpen = true;
        this.languagesDropdownOpen = false;
        this.searchResultsDropdownOpen = false;
        this.showSearchResults = true;
        this.openNavigationDrawer();
    },
    nextRole: function () {
        if (this.currentRoleIndex < this.roles.length - 1) {
            this.setCurrentRole(this.roles[this.currentRoleIndex + 1].id);
        }
    },
    previousRole: function () {
        if (this.currentRoleIndex > 0) {
            this.setCurrentRole(this.roles[this.currentRoleIndex - 1].id);
        }
    },
    toggleNavigationDrawer: function () {
        this.hideNavigationDrawer = false;
        this.navigationDrawerOpen = !this.navigationDrawerOpen;
    },
    closeNavigationDrawer: function () {
        this.hideNavigationDrawer = false;
        this.navigationDrawerOpen = false;
    },
    openNavigationDrawer: function () {
        this.hideNavigationDrawer = false;
    },
    changeLocale: function (locale) {
        this.locale = locale;
        AlpineI18n.locale = locale;
        this.dropdownOpen = false;
        this.languagesDropdownOpen = false;
        this.searchResultsDropdownOpen = false;
    },
    openLanguageDropdown: function () {
        this.dropdownOpen = true;
        this.languagesDropdownOpen = true;
        this.searchResultsDropdownOpen = false;
    },
    closeLanguageDropdown: function () {
        this.dropdownOpen = false;
        this.languagesDropdownOpen = false;
        this.searchResultsDropdownOpen = false;
    },
    closeSearchResultsDropdown: function () {
        this.dropdownOpen = false;
        this.languagesDropdownOpen = false;
        this.searchResultsDropdownOpen = false;
    },
    toggleFullscreen: function () {
        this.fullscreen = !this.fullscreen;
        if (this.fullscreen) {
            document.documentElement.requestFullscreen();
        } else {
            document.exitFullscreen();
        }
    },
    toggleDarkMode: function () {
        this.darkMode = !this.darkMode;
        if (this.darkMode) {
            document.documentElement.getElementsByTagName('body')[0].classList.remove('light');
            document.documentElement.getElementsByTagName('body')[0].classList.add('dark');
        } else {
            document.documentElement.getElementsByTagName('body')[0].classList.remove('dark');
            document.documentElement.getElementsByTagName('body')[0].classList.add('light');
        }
    },
    toggleNavigationDrawerVisibility: function () {
        this.hideNavigationDrawer = !this.hideNavigationDrawer;
    },
    openSearchForm: function () {
        this.showSearch = true;
        this.$nextTick(() => {
            document.getElementById('search-input').focus();
        });
    },
    closeSearchForm: function () {
        this.showSearch = false;
    },
    init() {
        this.wrapIxElements();
        this.addEventListeners();

        Array.from(document.querySelectorAll('.role-content')).forEach(element => {
            const role = this.roles.find(r => r.id === element.id);
            if (role) {
                role.contentElement = element;
            }
        });
    },

    searchFromCover() {
        if (this.searchTerm.length >= 3) {
            this.showCover = false;
            this.openSearchForm();
            this.search();
            this.openSearchResults();
            if (this.allResults.length > 0) {
                this.setCurrentRole(this.allResults[0].roleId, this.allResults[0].highlightId); 
            }
        }
    },

    search() {
        if (this.searchTerm.length < 3) {
            this.allResults = [];
            return;
        }
        const regex = new RegExp(this.searchTerm, 'gi');
        this.allResults = [];
        this.highlightCount = 0;
        this.showSearchResults = false;

        this.roles.forEach(role => {
            if (role.contentElement === null) return;

            // Reseteamos el contenido original para evitar múltiples resaltes en búsquedas consecutivas
            role.contentElement.innerHTML = role.contentElement.innerHTML.replace(/<span id="highlight-\d+" class="highlight">(.*?)<\/span>/g, '$1');

            // Función recursiva para recorrer todos los nodos de texto
            const highlightMatches = (node) => {
                if (node.nodeType === Node.TEXT_NODE) {
                    let contentText = node.textContent;
                    if (contentText.trim() === '') return;

                    const matches = [...contentText.matchAll(regex)];
                    if (matches.length > 0) {
                        const fragment = document.createDocumentFragment();
                        let lastIndex = 0;

                        matches.forEach((match, index) => {
                            this.highlightCount++;
                            const highlightId = `highlight-${this.highlightCount}`;
                            const matchIndex = match.index;
                            const matchLength = match[0].length;

                            // Crear nodo de texto antes de la coincidencia
                            if (matchIndex > lastIndex) {
                                fragment.appendChild(document.createTextNode(contentText.slice(lastIndex, matchIndex)));
                            }

                            // Crear el span resaltado
                            const highlightSpan = document.createElement('span');
                            highlightSpan.id = highlightId;
                            highlightSpan.className = 'highlight';
                            highlightSpan.textContent = match[0];
                            fragment.appendChild(highlightSpan);

                            lastIndex = matchIndex + matchLength;

                            // Añadir la coincidencia a los resultados
                            this.allResults.push({
                                roleId: role.id,
                                role: role,
                                labelKey: role.labelKey,
                                preview: contentText.slice(Math.max(0, matchIndex - 50), Math.min(contentText.length, matchIndex + matchLength + 50))
                                    .replace(regex, m => `<span class="highlight">${m}</span>`),
                                highlightId: highlightId
                            });
                        });

                        // Añadir el texto restante después de la última coincidencia
                        fragment.appendChild(document.createTextNode(contentText.slice(lastIndex)));
                        node.parentNode.replaceChild(fragment, node);
                    }
                } else if (node.nodeType === Node.ELEMENT_NODE) {
                    node.childNodes.forEach(childNode => highlightMatches(childNode));
                }
            };

            // Iniciar la búsqueda y resaltado en el elemento actual
            highlightMatches(role.contentElement);
        });

        this.currentPage = 1;
        this.searchResultsDropdownOpen = true;
        this.dropdownOpen = true;
        this.languagesDropdownOpen = false;
    },
    get paginatedResults() {
        const startIndex = (this.currentPage - 1) * this.resultsPerPage;
        const endIndex = startIndex + this.resultsPerPage;
        return this.allResults.slice(startIndex, endIndex);
    },
    get totalPages() {
        return Math.ceil(this.allResults.length / this.resultsPerPage);
    },
    prevPage() {
        if (this.currentPage > 1) this.currentPage--;
    },
    nextPage() {
        if (this.currentPage < this.totalPages) this.currentPage++;
    },
    toggleCollapsedResults(roleId) {
        const role = this.roles.find(r => r.id === roleId);
        if (role) {
            role.collapsedResults = !role.collapsedResults;
        }
    },
    wrapIxElements() {
        const ixElements = document.querySelectorAll('ix\\:nonFraction, ix\\:nonNumeric');
        ixElements.forEach((el, index) => {

            // check if parent is already a wrapper
            if (el.parentNode.classList.contains('ix-wrapper')) {
                return;
            }

            const wrapper = document.createElement('div');
            wrapper.className = 'ix-wrapper';
            wrapper.setAttribute('x-on:click.stop', `selectElement($event, ${index})`);

            const border = document.createElement('div');
            border.className = 'ix-border print:hidden ';

            if (el.parentNode.tagName === 'TD') {
                wrapper.classList.add('w-full');
                wrapper.classList.add('h-full');
            }

            el.parentNode.insertBefore(wrapper, el);
            wrapper.appendChild(el);
            wrapper.appendChild(border);


        });
    },

    addEventListeners() {
        document.addEventListener('click', () => {
            this.selectedElement = null;
            document.querySelectorAll('.ix-wrapper').forEach(el => {
                el.classList.remove('selected');
            });
        });
    },
    selectElement(event, index) {
        event.stopPropagation();
        if (this.selectedElement !== null) {
            this.selectedElement.classList.remove('selected');
        }
        this.selectedElement = event.currentTarget;
        this.selectedElement.classList.add('selected');
    },
    showCoverPage() {
        this.showCover = true;
    },
    showDocument() {
        this.showCover = false;
    }

};

function getRandomNumbers() {
    let dateObj = new Date()
    let dateTime = `${dateObj.getHours()}${dateObj.getMinutes()}${dateObj.getSeconds()}`
    return `${dateTime}${Math.floor((Math.random().toFixed(2) * 100))}`
}

document.addEventListener("alpine-i18n:ready", function () {
    window.AlpineI18n.create(locale, messages);
});

document.addEventListener('alpine:init', () => {

    reportStatus.init();

    Alpine.data('exportTableButton', (tableId) => ({
        showLabel: false,
        tableId: tableId,
        exportTable() {
            const table = document.getElementById(this.tableId);
            if (table) {
                var wb = XLSX.utils.table_to_book(table);
                XLSX.writeFile(wb, 'table_' + getRandomNumbers() + '.xlsx');
            }
        }
    }));
});

function addExportButtonsToTables() {
    const tables = document.getElementsByTagName('table');
    Array.from(tables).forEach((table, index) => {
        if (!table.id) {
            table.id = `table-${index}`;
        }

        const wrapper = document.createElement('div');
        wrapper.classList.add('overflow-x-auto');
        wrapper.classList.add('relative');
        table.parentNode.insertBefore(wrapper, table);
        wrapper.appendChild(table);

        const buttonHtml = `
            <div x-data="exportTableButton('${table.id}')"
                @mouseenter="showLabel = true"
                @mouseleave="showLabel = false"
                @click="exportTable"
                class="print:hidden absolute top-2 right-0 flex items-center cursor-pointer justify-center rounded-full bg-primary text-on-primary p-2 shadow-lg">
                <span x-show="showLabel"
                    x-transition:enter="transition ease-out duration-200"
                    x-transition:enter-start="opacity-0 transform scale-95"
                    x-transition:enter-end="opacity-100 transform scale-100"
                    x-transition:leave="transition ease-in duration-100"
                    x-transition:leave-start="opacity-100 transform scale-100"
                    x-transition:leave-end="opacity-0 transform scale-95"
                    class="text-sm text-gray-700 mx-2 text-on-primary "
                    x-text="$t('controls.exportTable')">
                </span>
                <div role="img"
                    class="mat-icon notranslate icon-size-6 mat-icon-no-color text-on-primary">
                    <svg fill="currentColor" viewBox="0 0 24 24" fit=""
                        height="100%" width="100%"
                        preserveAspectRatio="xMidYMid meet"
                        focusable="false">
                        <g>
                            <g>
                                <path
                                    d="M12,2C6.49,2,2,6.49,2,12s4.49,10,10,10s10-4.49,10-10S17.51,2,12,2z M12,20c-4.41,0-8-3.59-8-8s3.59-8,8-8s8,3.59,8,8 S16.41,20,12,20z M14.59,8.59L16,10l-4,4l-4-4l1.41-1.41L11,10.17V6h2v4.17L14.59,8.59z M17,17H7v-2h10V17z">
                                </path>
                            </g>
                        </g>
                    </svg>
                </div>
            </div>
        `;

        wrapper.insertAdjacentHTML('afterbegin', buttonHtml);
    });
}

function prepareForPrint() {
    // Guardar las referencias originales
    const originalElements = [];
    // Guardar las referencias de las clases originales de los wrappers
    const originalWrapperClasses = [];
    const elements = document.querySelectorAll('ix\\:nonNumeric');
    
    // Reemplazar cada ix:nonNumeric con un div
    elements.forEach(element => {

        // Buscar el padre con clase ix-wrapper
        const wrapper = element.parentElement;
        if (wrapper && wrapper.classList.contains('ix-wrapper')) {
            // Guardar referencia de las clases originales
            originalWrapperClasses.push({
                element: wrapper,
                classes: wrapper.className
            });
            
            // Remover la clase ix-wrapper
            wrapper.classList.remove('ix-wrapper');
        }

        const div = document.createElement('div');
        div.innerHTML = element.innerHTML;
        
        // Guardar referencia para restaurar después
        originalElements.push({
            original: element,
            replacement: div
        });
        
        // Reemplazar el elemento
        element.parentNode.replaceChild(div, element);
    });
    
    // Imprimir
    window.print();
    
    // Restaurar los elementos originales después de imprimir
    window.onafterprint = function() {
        originalElements.forEach(({original, replacement}) => {
            replacement.parentNode.replaceChild(original, replacement);
        });

        originalWrapperClasses.forEach(({element, classes}) => {
            element.className = classes;
        });
    };
}

// Run the function when the DOM is fully loaded
document.addEventListener('DOMContentLoaded', addExportButtonsToTables);