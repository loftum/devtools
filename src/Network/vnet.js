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

class VNet {
    /**
     * @param name {string}
     * @param range {IpRange}
     */
    constructor(name, range) {
        this._eventListeners = [];
        this._name = name;
        this._range = range;
    }
    
    addEventListener(listener) {
        this._eventListeners.push(listener);
    }

    /**
     * 
     * @returns {VNet}
     */
    get left() {
        return this._left;
    }

    /**
     * 
     * @returns {VNet}
     */
    get right() {
        return this._right;
    }

    /**
     * 
     * @param name {string}
     * @param cidr {string}
     */
    add(name, cidr) {
        const range = IpRange.parse(cidr);
        if (!this.range.contains(range)) {
            return;
        }
        
        if (this.range.equals(range)) {
            this.name = name;
            return;
        }
        
        const subs = this.split();
        subs.left.add(name, cidr);
        subs.right.add(name, cidr);
    }
    
    split() {
        const subs = this.range.split();
        if (!subs) {
            return;
        }

        if (!this._left) {
            this._left = new VNet(undefined, subs[0]);
        }
        
        if (!this._right) {
            this._right = new VNet(undefined, subs[1]);
        }
        
        return {
            left: this._left,
            right: this._right
        };
    }

    /**
     * 
     * @returns {string}
     */
    get name() {
        return this._name;
    }
    
    set name(value) {
        const oldValue = this._name;
        this._name = value;
        this.notify("namechange", oldValue, value);
    }
    
    notify(eventName, oldValue, newValue) {
        for (let ii=0; ii<this._eventListeners.length; ii++) {
            this._eventListeners[ii].on(eventName, oldValue, newValue);
        }
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
     * @returns {string}
     */
    info() {
        if (!this.name) {
            return this.range.info();
        }
        return `${this.name} ${this.range.info()}`;
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
     * 
     * @returns {VNet|*}
     */
    get vnet() {
        return this._vnet;
    }
    
    set vnet(value) {
        this._vnet = value;
        value.addEventListener(this);
        this.render();
    }
    
    on(eventName, oldValue, newValue) {
        switch(eventName){
            case "namechange":
                if (newValue) {
                    
                }
        }
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
        range.innerHTML = this.vnet?.range?.info();
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
        if (!this._initiated){
            return ;
        }
        
        if (!this.vnet) {
            this._title.innerHTML = "";
            return;
        }
       
        if (this.vnet.name) {
            this.classList.add("registered");
        }
        else {
            this.classList.remove("registered");
        }
        
         if (!this.parent){
            const dimension = this.dimension;
            this.style.position = "absolute";
            this.style.display = "block";
            this.style.height = `${dimension.height}px`;
            this.style.width = `${dimension.width}px`;
        }
        
        this._title.innerHTML = this.vnet.name || "";
        
        if (this.vnet.left) {
            if (!this._left) {
                const left = new VNetElement();
                left.parent = this;
                left.vnet = this.vnet.left;
                this._container.appendChild(left);
                this._left = left;
            }
        }
        
        if (this.vnet.right) {
            if (!this._right) {
                const right = new VNetElement();
                right.parent = this;
                right.vnet = this.vnet.right;
                this._container.appendChild(right);
                this._right = right;
            }
        }
    }
    
    startEditName() {
        const input = document.createElement("input");
        input.type = "text";
        input.value = this.vnet?.name ?? "";
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
                this.vnet.name = value;
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
        const container = this._container;
        if (this._left || this._right) {
            this._left = undefined;
            this._right = undefined;
            container.innerHTML = "";
            return;
        }
        this.split();
    }
    
    split() {
        if (this._left || this._right) {
            return;
        }
        
        const subs = this.vnet.split();
        this._left = new VNetElement();
        this._left.parent = this;
        this._left.vnet = subs.left;
        this._container.appendChild(this._left);

        this._right = new VNetElement();
        this._right.parent = this;
        this._right.vnet = subs.right;
        this._container.appendChild(this._right);
    }
    
    get dimension() {
        const log2 = Math.log2(this.vnet.range.size);
        if (log2 % 2 === 0){
            return Math.sqrt(this.vnet.range.size);
        }
        const short = Math.sqrt(this.vnet.range.size / 2);
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

    }

    attributeChangedCallback(name, oldValue, newValue) {
        switch(name) {
            case "cidr":
                const range = IpRange.parse(newValue);
                if (!range){
                    return;
                }
                this.vnet = new VNet(undefined, range);
                break;
        }
    }
}

customElements.define('v-net', VNetElement);