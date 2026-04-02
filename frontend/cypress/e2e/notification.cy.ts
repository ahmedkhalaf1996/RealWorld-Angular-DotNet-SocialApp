describe("Notifications page", ()=>{
  beforeEach(()=>{
    cy.visit("/notifications");
  });

  it("should load the notification page", ()=>{
    cy.get("app-root").should('exist');
  });

  it("should render with out  erros", ()=>{
    cy.get('body').should('exist');
  })
})

