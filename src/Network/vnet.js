const uintMax = 4294967295

class IpRange {
    
    constructor(address, cidrBits) {
        this.address = address;
        this.cidrBits = cidrBits;
        
        let mask = 0;
        for (let ii=0; ii<cidrBits; ii++) {
            mask |= (1 << (31-ii));
        }
        this._min = address;
        this._max = this._min | (mask ^ uintMax); // uint.Max
        if (this._max < 0) {
            // bit overflow. Happens for 0.0.0.0/0
            this._max = uintMax - this._max - 1;
        }
    }
    
    find(cidr) {
        const range = IpRange.parse(cidr);
        if (!range) {
            return undefined;
        }
        if (!this.contains(range)) {
            return undefined;
        }
        if (this.equals(range)) {
            return this;
        }
        
        const subs = this.splitInto(2);
        
    }

    /**
     * 
     * @param range {IpRange}
     */
    equals(range) {
        return this.min === range.min &&
            this.max === range.max;
    }

    /**
     * 
     * @param range {IpRange}
     */
    contains(range) {
        return range.min >= this.min && range.max <= this.max;
    }
    
    /**
     * @returns {number}
     */
    get address() {
        return this._address;
    }

    set address(value) {
        this._address = value;
    }

    /**
     * @returns {number}
     */
    get cidrBits(){
        return this._cidrBits;
    }
    
    get min(){
        return this._min;
    }
    
    get max() {
        return this._max;
    }
    
    get size() {
        return this.max - this.min + 1;
    }

    set cidrBits(value) {
        this._cidrBits = value;
    }
    
    static parse(value) {
        let octets = [0, 0, 0, 0];
        let builder = "";

        let octetIndex = 0;
        let cidrIndex = 0;

        for (let ii=0; ii<value.length; ii++) {
            const c = value[ii];

            cidrIndex++;
            switch (c) {
                case '.':
                    if (octetIndex >= 3) {
                        return undefined;
                        //throw `Invalid CIDR '${value}'. Four octets is enough at ${cidrIndex}`;
                    }
                    octets[octetIndex] = parseInt(builder);
                    builder = "";
                    octetIndex++;
                    break;
                case '/':
                    if (octetIndex < 3) {
                        return undefined;
                        //throw `Invalid CIDR '${value}'. Expected more numbers at ${cidrIndex}`;
                    }

                    octets[octetIndex] = parseInt(builder);
                    builder = "";
                    octetIndex++;
                    break;
                default:
                    builder += c;
                    break;
            }
        }

        if (octetIndex !== 4 && builder.length > 0)
        {
            octets[octetIndex] = parseInt(builder);
            builder = "";
            octetIndex++;
        }

        if (octetIndex !== 4)
        {
            return undefined;
            //throw `Invalid CIDR '${value}'. Expected more numbers at ${cidrIndex}`;
        }
        const address = (octets[0] << 24) |
            (octets[1] << 16)|
            (octets[2] << 8) |
            (octets[3] << 0);
        return new IpRange(address, builder.length > 0 ? parseInt(builder) : 255);
    }
    
    split() {
        return this.splitInto(2);
    }
    
    splitInto(number) {
        if (number % 2 !== 0) {
            throw `Cannot split into ${number}. Must be positive and dividable by 2`;
        }

        const cidrBits = this._cidrBits + Math.log2(number);

        if (cidrBits > 32) {
            return undefined;
        }
        
        const chunk = this.size / number;
        let subRanges = [];
        for (let ii=0; ii< number; ii++) {
            subRanges.push(new IpRange(this.min + ii * chunk, cidrBits));
        }
        return subRanges;
    }

    get value() {
        return this.format();
    }
    
    format() {
        const address = this.address;
        const octets = [
            (address >> 24) & 0xff,
            (address >> 16) & 0xff,
            (address >> 8) & 0xff,
            (address & 0xff)
        ];
        
        const ip = octets.join('.');
        return `${ip}/${this.cidrBits}`;
    }
    
    info() {
        return `${this.format()} (${this.size})`
    }
}

class VNetElement extends HTMLElement {

    static observedAttributes = ["cidr"];

    constructor() {
        super();
        this._left = undefined;
        this._right = undefined;
        this.addEventListener("mouseover", () => this.classList.add("hovered"));
        this.addEventListener("mouseout", () => this.classList.remove("hovered"));
    }
    
    highlight(e) {
        if (this.range.equals(e.range)){
            this.classList.add("highlighted");
        }
    }
    
    unhighlight(e) {
        if (this.range.equals(e.range)){
            this.classList.remove("highlighted");
        }
    }
    /**
     * @returns {string}
     */
    get direction() {
        const parent = this.parent;
        if (!parent) {
            return "horizontal";
        }
        const parentValue = parent.direction;
        switch(parentValue) {
            case "vertical":
                return "horizontal";
            case "horizontal":
                return "vertical";
            default:
                return "horizontal";
        }
    }

    get parent() {
        return this._parent;
    }

    set parent(parent) {
        this._parent = parent;
    }

    /**
     * @returns {VNetElement | undefined}
     */
    get left(){
        return this.left;
    }
    set left(value) {
        value.parent = this;
        value.model = this.model;
        value.range = this.range.split()[1];
        this._left = value;
        this.appendChild(value);
    }

    /**
     * @returns {VNetElement | undefined}
     */
    get right(){
        return this._right;
    }
    
    set right(value) {
        value.parent = this;
        value.model = this.model;
        value.range = this.range.split()[0];
        this._right = value;
        this.appendChild(value);
    }

    split() {
        if (this._left || this._right) {
            return;
        }

        const subs = this.range.split();
        this._left = new VNetElement();
        this._left.parent = this;
        this._left.range = subs[0];
        this._left.model = this.model;
        this._container.appendChild(this._left);
        
        
        this._right = new VNetElement();
        this._right.parent = this;
        this._right.range = subs[1];
        this._right.model = this.model;
        this._container.appendChild(this._right);
        
    }
    
    removeChildren() {
        if (this._left) {
            this._left.remove();
            this._left = undefined;
        }
        if (this._right) {
            this._right.remove();
            this._right = undefined;
        }
        
        this._container.innerHTML = "";
    }

    /**
     * 
     * @returns {IpRange}
     */
    get range(){
        return this._range;
    }
    
    set range(value) {
        this._range = value;
    }

    /**
     * 
     * @returns {string | undefined}
     */
    get name() {
        return this._name;
    }
    
    set name(value) {
        if (value) {
            this.model.add(value, this.range.format());
        }
        else {
            this.model.remove(this._name, this.range.format());
        }
        this._name = value;
    }
    
    /**
     * @returns {VNetModel}
     */
    get model() {
        return this._model;
    }
    
    set model(value) {
        this._model = value;
        value.addEventListener("add", this.render.bind(this));
        value.addEventListener("remove", this.render.bind(this));
        value.addEventListener("highlight", this.highlight.bind(this));
        value.addEventListener("unhighlight", this.unhighlight.bind(this));
        this.render();
    }
    
    connectedCallback() {
        const overflow = "visible";
        
        this.style.display = "flex";
        this.style.alignItems = "stretch";
        this.style.alignContent = "stretch";
        this.style.flexWrap = "nowrap";
        this.style.flexDirection = "column";
        this.style.boxSizing = "border-box";
        this.style.overflow = overflow;

        const titleBar = document.createElement("div");
        titleBar.style.display = "flex";
        titleBar.style.flexDirection = "row";
        titleBar.style.justifyContent = "space-between";
        titleBar.style.overflow = overflow;

        this.appendChild(titleBar);

        const title = document.createElement("div");
        title.innerHTML = this.vnet?.name ?? "";
        titleBar.appendChild(title);
        this._title = title;
        
        const range = document.createElement("span");
        range.innerHTML = this.range?.info();
        titleBar.appendChild(range);
        
        titleBar.addEventListener("dblclick", this.startEditName.bind(this));

        const button = document.createElement("button");
        button.style.cursor = "pointer";
        button.innerHTML = "/";
        button.addEventListener("click", this.clicked.bind(this));
        titleBar.append(button);

        const direction = this.direction;
        switch(direction) {
            case "horizontal":
                 this.style.height = "100%";
                break;
            case "vertical":
                this.style.width = "100%";
                break;
        }
        // this.style.flexGrow = "1";
        
        const container = document.createElement("div");
        container.style.display = "flex";
        container.style.alignItems = "stretch";
        container.style.alignContent = "stretch";
        container.style.justifyContent = "center";
        container.style.flexWrap = "nowrap";
        container.style.flexDirection = direction === "horizontal" ? "row" : "column";
        container.style.height = "100%";
        container.style.boxSizing = "border-box";
        container.style.overflow = overflow;
        this.appendChild(container);
        this._container = container;
        this._initiated = true;
        this.render();
    }
    
    _initiated = false;
    
    render() {
        if (!this._initiated) {
            return ;
        }
        
        if (!this.model) {
            this._title.innerHTML = "";
            return;
        }

        this._name = this.model.getName(this.range);
        this._title.innerHTML = this._name || "";
        
        if (this._name) {
            this.classList.add("registered");
        }
        else {
            this.classList.remove("registered");
        }
        
        if (!this.parent) {
            const dimension = this.dimension;
            this.style.position = "absolute";
            this.style.display = "block";
            this.style.height = `${dimension.height}px`;
            this.style.width = `${dimension.width}px`;
        }
        
        if (this.model.hasSubnetsOf(this.range)) {
            this.split();
        }
        else {
            this.removeChildren();
        }
    }
    
    startEditName() {
        const input = document.createElement("input");
        input.type = "text";
        input.value = this.name ?? "";
        input.addEventListener("keyup", this.endEditName.bind(this));
        this._title.innerHTML = "";
        this._title.appendChild(input);
    }

    /**
     * 
     * @param e {KeyboardEvent}
     */
    endEditName(e) {
        switch(e.key){
            case "Enter":
            case "Return":
                const value = e.target.value;
                this.name = value;
                e.target.removeEventListener("keyup", this.endEditName.bind(this));
                this.render();
                break;
            case "Escape":
                e.target.removeEventListener("keyup", this.endEditName.bind(this));
                this.render();
                break;
        }
    }

    clicked() {
        if (this._left || this._right) {
            this.removeChildren();
            return;
        }
        this.split();
    }
    
    remove() {
        if (this.name) {
            this.model.remove(this.name, this.range.format());
        }
        if (this._left){
            this._left.remove();
        }
        if (this._right) {
            this._right.remove();
        }
    }
    
    get dimension() {
        const log2 = Math.log2(this.range.size);
        if (log2 % 2 === 0) {
            const side = Math.sqrt(this.range.size);
            return {
                height: side,
                width: side
            };
        }
        const short = Math.sqrt(this.range.size / 2);
        const long = short * 2;
        
        switch(this.direction) {
            case "horizontal":
                return {
                    height: short,
                    width: long
                };
            default:
                return {
                    height: long,
                    width: short
                };
        }
    }

    disconnectedCallback() {
        if (this.model) {
            this.model.removeEventListener("add", this.render.bind(this));
            this.model.removeEventListener("remove", this.render.bind(this));
        }
    }

    attributeChangedCallback(name, oldValue, newValue) {
        switch(name) {
            case "cidr":
                const range = IpRange.parse(newValue);
                if (!range){
                    return;
                }
                this.range = range;
                break;
        }
    }
}

customElements.define('v-net', VNetElement);

class VNetTableElement extends HTMLElement {
    
    constructor() {
        super();
        this.addEventListener("paste", this.paste.bind(this));
    }
    
    paste(e) {
        console.log("PASTE!");
        
        const text = e.clipboardData.getData("text/plain");
        const lines = text.split("\n");
        const entries = [];
        for (let ii=0; ii<lines.length; ii++) {
            const line = lines[ii];
            const parts = line.split(",");
            
            if (parts.length < 3) {
                continue;
            }
            
            const entry = VNetEntry.parse(parts[0], parts[2]);
            
            if (entry) {
                entries.push({name: name, cidr: parts[2]});    
            }
        }
        
        if (entries.length === 0) {
            return;
        }
        this.model.entries = entries;
    }

    /**
     * @returns {VNetModel}
     */
    get model() {
        return this._model;
    }
    
    set model(value) {
        
        this._model = value;
        value.addEventListener("add", this.render.bind(this));
        value.addEventListener("remove", this.render.bind(this));
        this.render();
    }
    
    highlight(e) {
        
    }

    connectedCallback() {
        this.innerHTML = `
        <table>
        <thead>
        <tr>
            <th>name</th>
            <th>cidr</th>
            <th>size</th>
            <th></th>
        </tr>
        </thead>
        <tbody>
        </tbody>
        </table>
        `;
        this._tbody = this.getElementsByTagName("tbody")[0];

        const tr = document.createElement("tr");

        tr.innerHTML = `
        <td><input type="text" placeholder="name"></td>
        <td><input type="text" placeholder="10.10.0.0/16"></td>
        <td></td>
        <td><button>+</button></td>`;
        
        const nameInput = tr.getElementsByTagName("input")[0];
        const cidrInput = tr.getElementsByTagName("input")[1];
        const addButton = tr.getElementsByTagName("button")[0];
        addButton.addEventListener("click", () => {
            this.model.add(nameInput.value, cidrInput.value);
        });
        this._tr = tr;
        
        this._initiated = true;
        this.render();
    }
    
    _initiated = false;
    
    render() {
        
        if (!this._initiated || !this.model) {
            return;
        }
        
        this._tbody.innerHTML = "";
        for (let ii=0; ii<this.model.entries.length; ii++) {
            const entry = this.model.entries[ii];
            const tr = document.createElement("tr");
            tr.innerHTML = `<td>${entry.name}</td><td>${entry.range.format()}</td><td>${entry.range.size}</td><td><button>D</button></td>`;
            const deleteButton = tr.getElementsByTagName("button")[0];
            deleteButton.addEventListener("click", () => {
                this.model.removeEntry(entry);
            });
            tr.addEventListener("mouseenter", () => {
                tr.classList.add("highlighted");
                this.model.highlight(entry);
            });
            tr.addEventListener("mouseleave", () => {
                tr.classList.remove("highlighted");
                this.model.unhighlight(entry);
            });
            this._tbody.appendChild(tr);
        }
        
        this._tbody.appendChild(this._tr);
    }
}

customElements.define('v-net-table', VNetTableElement);

class VNetEntry {

    /**
     * @param name {string}
     * @param range {IpRange}
     */
    constructor(name, range) {
        this.name = name;
        this.range = range;
    }
    /**
     * @returns {string}
     */
    get name(){
        return this._name;
    }
    set name(value) {
        this._name = value;
    }

    /**
     * @returns {IpRange}
     */
    get range() {
        return this._range;
    }
    
    set range(value) {
        this._range = value;
    }

    static parse(name, cidr) {
        if (!name) {
            return undefined;
        }
        const range = IpRange.parse(cidr);
        if (!range) {
            return undefined;
        }
        
        return new VNetEntry(name, range);
    }
}

class VNetModel {
    
    constructor(entries) {
        entries = entries || [];
        /**
         * @type {[VNetEntry]}
         * @private
         */
        this._entries = [];
        for(let ii=0; ii<entries.length; ii++) {
            const e = entries[ii];
            const entry = VNetEntry.parse(e.name, e.cidr);
            if (!entry){
                continue;
            }
            this._entries[ii] = entry;
        }
        
        const listeners = new Map();
        listeners.set("add", []);
        listeners.set("remove", []);
        listeners.set("highlight", []);
        listeners.set("unhighlight", []);
        this._eventListeners = listeners;
    }

    /**
     * @returns {[VNetEntry]}
     */
    get entries() {
        return this._entries;
    }

    /**
     * 
     * @param value {[VNetEntry]}
     */
    set entries(value) {
        this._entries = value;
        this.fire("add", value);
    }

    /**
     * @param range {IpRange}
     */
    hasSubnetsOf(range) {
        
        for (let ii=0; ii<this._entries.length; ii++) {
            const element = this._entries[ii];
            
            if (range.contains(element.range) && !range.equals(element.range)) {
                return true;
            }
        }
        
        return false;
    }
    
    clear() {
        while(this._entries.length > 0){
            this._entries.pop();
        }
        this.fire("remove", this._entries);
    }
    
    highlight(entry) {
        this.fire("highlight", entry);
    }
    
    unhighlight(entry) {
        this.fire("unhighlight", entry);
    }
    
    /**
     * @param name {string}
     * @param cidr {string}
     */
    add(name, cidr) {
        const entry = VNetEntry.parse(name, cidr);
        if (!entry){
            return;
        }
        
        this._entries.push(entry);
        this.fire("add", entry);
    }
    
    remove(name, range) {
        let ii = 0;
        while (ii < this._entries.length) {
            const entry = this._entries[ii];
            if (entry.name === name && entry.range.equals(range)) {
                this._entries.splice(ii, 1);
                this.save();
                this.fire("remove", entry);
            }
            else {
                ii++;
            }
        }
    }
    
    removeEntry(entry) {
        const index = this._entries.indexOf(entry);
        if (index < 0) {
            return;
        }
        this._entries.splice(index, 1);
        this.fire("remove", entry);
    }
    
    getName(range) {
        const entries = this._entries.filter(e => e.range.equals(range));
        switch(entries.length){
            case 0:
                return undefined;
            case 1:
                return entries[0].name;
            default:
                return entries.map(e => e.name).join(", ");
                
        }
    }
    
    fire(eventName, value) {
        const listeners = this._eventListeners.get(eventName);
        for(let ii=0; ii<listeners.length; ii++) {
            listeners[ii](value);
        }
    }
    
    addEventListener(eventName, callback) {
        if (!this._eventListeners.has(eventName)){
            return;
        }
        this._eventListeners.get(eventName).push(callback);
    }
    
    removeEventListener(eventName, callback) {
        if (!this._eventListeners.has(eventName)){
            return;
        }
        const listeners = this._eventListeners.get(eventName);
        const index = listeners.indexOf(callback);
        if (index < 0){
            return;
        }
        listeners.splice(index, 1);
    }
    
    save() {
        const elements = this._entries.map(e => {
            return {
                name: e.name,
                cidr: e.range.format()    
            }
        });
        window.localStorage.setItem("vnet-table", JSON.stringify(elements));
    }
    
    static load() {
        const json = window.localStorage.getItem("vnet-table") || "[]";
        return new VNetModel(JSON.parse(json));
    }
}