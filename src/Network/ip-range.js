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

class IpRangeElement extends HTMLElement {
    
    static observedAttributes = ["cidr"];
    
    constructor() {
        super();
        this.cidr = "";
        
        this.addEventListener("mouseover", () => this.style.backgroundColor = "lightblue");
        this.addEventListener("mouseout", () => this.style.backgroundColor = "");
    }
    
    set cidr(value) {
        this.range = IpRange.parse(value);
    }

    /**
     * @returns {string}
     */
    get cidr(){
        return this.range?.info() ?? "unknown";
    }

    /**
     * @returns {IpRange|undefined}
     */
    get range() {
        return this._range;
    }
    
    set range(range) {
        this._range = range;
    }
    
    get parent() {
        return this._parent;
    }
    
    set parent(parent) {
        this._parent = parent;
    }

    /**
     * @returns {string}
     */
    get direction() {
        const parent = this.parent;
        if (!parent){
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
    
    click() {
        const container = this._container;
        if (container) {
            this.removeChild(container);
            this._container = undefined;
            return;
        }
        else {
            this.split();
        }
    }
    
    connectedCallback() {
        this.style.display = "flex";
        this.style.alignItems = "stretch";
        this.style.alignContent = "stretch";
        this.style.flexWrap = "nowrap";
        this.style.flexDirection = "column";
        this.style.margin = this.style.margin || "3px";
        this.style.boxSizing = "border-box";
        
        const titleBar = document.createElement("div");
        titleBar.style.display = "flex";
        titleBar.style.flexDirection = "row";
        titleBar.style.justifyContent = "space-between";
                
        this.appendChild(titleBar);
        
        const title = document.createElement("div");
        title.innerHTML = this.range?.info();
        titleBar.appendChild(title);
        titleBar.addEventListener("dblclick", this.click.bind(this));
        
        const button = document.createElement("button");
        button.style.cursor = "pointer";
        button.innerHTML = "/";
        button.addEventListener("click", this.click.bind(this));
        titleBar.append(button);
        
        const direction = this.direction;
        switch(direction) {
            case "horizontal":
                this.style.height = "100%";
                break;
            case "vertical":
                this.style.width = "100%";
        }
    }

    split() {
        
        if (this._container) {
            return;
        }

        const range = this._range;
        if (!range) {
            return;
        }
        const parts = range.split();

        if (!parts) {
            console.log("Could not split");
            return;
        }
        const [range1, range2] = parts;
        

        const container = document.createElement("div");
        this.appendChild(container);
        container.style.display = "flex";
        container.style.alignItems = "stretch";
        container.style.alignContent = "stretch";
        container.style.flexWrap = "nowrap";
        container.style.height = "100%";
        container.style.width = "100%";
        const direction = this.direction;
        switch(direction) {
            case "horizontal":
                container.style.flexDirection = "row";
                break;
            case "vertical":
                container.style.flexDirection = "column";
        }

        const sub1 = new IpRangeElement();
        sub1.parent = this;
        sub1.range = range1;
        container.appendChild(sub1);
        
        const sub2 = new IpRangeElement();
        sub2.parent = this;
        
        sub2.range = range2;
        container.appendChild(sub2);
        this._container = container;
    }
    
    disconnectedCallback() {
        
    }
    
    attributeChangedCallback(name, oldValue, newValue) {
        switch(name) {
            case "cidr":
                this.cidr = newValue;
        }
    }
}

customElements.define('ip-range', IpRangeElement);

class VNet {
    /**
     * 
     * @param range {IpRange}
     */
    constructor(name, range) {
        this.name = name;
        this.range = range;
        this._children = [];
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
     * @returns {[VNet]}
     */
    get children(){
        return this._children;
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
        
        const subs = this.range.split();
        if (!subs) {
            return;
        }
        
        if (!this._left) {
            this._left = new VNet(undefined, subs[0]);
        }
        this._left.add(name, cidr);
        if (!this._right) {
            this._right = new VNet(undefined, subs[1]);
        }
        this._right.add(name, cidr);
    }

    /**
     * 
     * @returns {string}
     */
    get name() {
        return this._name;
    }
    
    set name(value) {
        this._name = value;
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
        title.innerHTML = this._vnet?.info();
        titleBar.appendChild(title);
        this._title = title;
        titleBar.addEventListener("dblclick", this.click.bind(this));

        const button = document.createElement("button");
        button.style.cursor = "pointer";
        button.innerHTML = "/";
        button.addEventListener("click", this.click.bind(this));
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
        
        
        
        this._title.innerHTML = this.vnet.info();
        
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