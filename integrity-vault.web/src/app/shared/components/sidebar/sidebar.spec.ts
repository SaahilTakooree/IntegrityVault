// Import dependencies.
import { ComponentFixture, TestBed } from "@angular/core/testing"; // Import Angular testing utilities.
import { SidebarComponent } from "./sidebar"; // Import the component being tested.
import { By } from "@angular/platform-browser"; // Utility to query the DOM.
import { AuthService } from "../../../core/services/auth/auth.service"; // Import the service to be mocked.


describe("SidebarComponent", () => {
    // Component instance and testing fixture.
    let component: SidebarComponent;
    let fixture: ComponentFixture<SidebarComponent>;
    let authServiceSpy: jasmine.SpyObj<AuthService>;


    beforeEach(async () => {
        // Create a spy object for AuthService.
        authServiceSpy = jasmine.createSpyObj("AuthService", ["logout"]);

        // Set up testing module.
        await TestBed.configureTestingModule({
            imports: [SidebarComponent],
            providers: [
                { provide: AuthService, useValue: authServiceSpy }
            ]
        }).compileComponents();

        // Create component instance.
        fixture = TestBed.createComponent(SidebarComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });


    // Basic instantiation check.
    it("Should create the component.", () => {
        expect(component).toBeTruthy();
    });


    // Verifies that the 'collapsed' class is applied correctly based on the input.
    it("Should apply 'collapsed' class when isCollapsed is true.", () => {
        component.isCollapsed = true;
        fixture.detectChanges();

        const aside = fixture.debugElement.query(By.css("aside"));
        expect(aside.nativeElement.classList).toContain("collapsed");
    });


    // Confirms that sidebar items are rendered correctly and labels are displayed.
    it("Should render the list of sidebar items.", () => {
        component.sidebarItems = [
            { label: "Dashboard", icon: "bi-house", link: "/dashboard" },
            { label: "Users", icon: "bi-people", link: "/users" }
        ];
        fixture.detectChanges();

        // Target only the links inside the nav element
        const navLinks = fixture.debugElement.queryAll(By.css("nav .nav-link"));
        
        expect(navLinks.length).toBe(2);
        expect(navLinks[0].nativeElement.textContent).toContain("Dashboard");
        expect(navLinks[1].nativeElement.textContent).toContain("Users");
    });

    // Checks the setter validation: should throw an error if a label exceeds 24 characters.
    it("Should throw an error if a sidebar label exceeds 24 characters.", () => {
        const longItem = [{ label: "This label is definitely way too long for the sidebar", link: "/test" }];
        
        expect(() => {
            component.sidebarItems = longItem;
        }).toThrowError(/exceeds 15 characters/); // Matching the specific error message in your code.
    });


    // Ensures the active class is applied to the link matching activeLink input.
    it("Should highlight the active link.", () => {
        component.sidebarItems = [{ label: "Home", link: "/home" }];
        component.activeLink = "/home";
        fixture.detectChanges();

        const activeLink = fixture.debugElement.query(By.css(".nav-link.active"));
        expect(activeLink).toBeTruthy();
        expect(activeLink.nativeElement.textContent).toContain("Home");
    });


    // Verifies that clicking an item emits the linkClicked event.
    it("Should emit linkClicked when a menu item is clicked.", () => {
        spyOn(component.linkClicked, "emit");
        component.sidebarItems = [{ label: "Profile", link: "/profile" }];
        fixture.detectChanges();

        const link = fixture.debugElement.query(By.css(".nav-link"));
        link.triggerEventHandler("click", null);

        expect(component.linkClicked.emit).toHaveBeenCalledWith("/profile");
    });


    // Confirms that the closeSidebar event is emitted when the toggle button is clicked.
    it("Should emit closeSidebar when the close button is clicked.", () => {
        spyOn(component.closeSidebar, "emit");
        
        const closeBtn = fixture.debugElement.query(By.css(".iv-sidebar-toggle"));
        closeBtn.triggerEventHandler("click", null);

        expect(component.closeSidebar.emit).toHaveBeenCalled();
    });


    // Validates that the logout method of AuthService is called when the logout link is clicked.
    it("Should call authService.logout when the logout link is clicked.", () => {
        const logoutLink = fixture.debugElement.query(By.css(".logout-link"));
        logoutLink.triggerEventHandler("click", null);

        expect(authServiceSpy.logout).toHaveBeenCalled();
    });


    // Checks the empty state when no sidebar items are provided.
    it("Should show 'No menu items available' when sidebarItems is empty.", () => {
        component.sidebarItems = [];
        fixture.detectChanges();

        const emptyMessage = fixture.debugElement.query(By.css(".text-muted"));
        expect(emptyMessage.nativeElement.textContent).toContain("No menu items available");
    });
});