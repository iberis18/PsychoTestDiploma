import React, { Component } from 'react';
import { Button, Row, Col, Input, InputGroup, InputGroupAddon, Pagination, PaginationItem, PaginationLink } from 'reactstrap';


export class CustomPagination extends Component {
    static displayName = CustomPagination.name;

    constructor(props) {
        super(props);
        this.state = {
            pageCount: 0,
            currentPage: 1,
            firstPage: 1,
            secondPage: 2,
            thirdPage: 3
        };
        var changeCurrentPage = false;
        this.onClick = this.onClick.bind(this);
    }

    componentDidMount() {
        this.getCountOfPage();
    }

    async getCountOfPage() {
        const token = sessionStorage.getItem('tokenKey');
        if (this.changeCurrentPage === true)
            this.setState({ currentPage: 1, firstPage: 1, secondPage: 2, thirdPage: 3 });
        if (this.props.controllerUrl !== "") {
            var response = await fetch(this.props.controllerUrl + "pageCount/" + this.props.postfixUrl, {
                method: "GET",
                headers: {
                    "Accept": "application/json",
                    "Authorization": "Bearer " + token
                }
            });
            var data = await response.json();
            if (response.ok === true) {
                this.setState({ pageCount: data });
            }
            else {
                console.log("Error: ", response.status);
            }
        }
    }

    componentDidUpdate() {
        this.getCountOfPage();
    }
    shouldComponentUpdate(nextProps, nextState) {
        if (this.state.pageCount != nextState.pageCount || this.props.controllerUrl != nextProps.controllerUrl || this.props.postfixUrl != nextProps.postfixUrl)
            this.changeCurrentPage = true;
        else this.changeCurrentPage = false;
        return this.state.pageCount != nextState.pageCount || this.props.controllerUrl != nextProps.controllerUrl ||
            this.props.postfixUrl != nextProps.postfixUrl || this.state.currentPage != nextState.currentPage
    }

    onClick(newPage) {
        if (this.state.pageCount > 3)
            if (newPage <= 1) {
                this.setState({ firstPage: 1, secondPage: 2, thirdPage: 3 });
                newPage = 1;
            }
            else
                if (newPage >= this.state.pageCount) {
                    this.setState({ firstPage: this.state.pageCount - 2, secondPage: this.state.pageCount - 1, thirdPage: this.state.pageCount });
                    newPage = this.state.pageCount;
                }
                else
                    this.setState({ firstPage: newPage - 1, secondPage: newPage, thirdPage: newPage + 1 });
        else
            if (newPage <= 1) newPage = 1;
            else if (newPage >= this.state.pageCount) newPage = this.state.pageCount;

        this.props.setCurrentPage(newPage);
        this.setState({ currentPage: newPage }, () => {
            this.props.getContent(this.props.controllerUrl + "page/" + this.state.currentPage + "/" + this.props.postfixUrl);
        });
    }

    render() {
        if (this.state.pageCount > 2)
            return (
                <div>
                    <Pagination>
                        <PaginationItem>
                            <PaginationLink previous onClick={() => { this.onClick(this.state.currentPage - 1) }} />
                        </PaginationItem>

                        <PaginationItem active={this.state.firstPage === this.state.currentPage} >
                            <PaginationLink onClick={() => { this.onClick(this.state.firstPage) }}>{this.state.firstPage}</PaginationLink>
                        </PaginationItem>
                        <PaginationItem active={this.state.secondPage === this.state.currentPage}>
                            <PaginationLink onClick={() => { this.onClick(this.state.secondPage) }}>{this.state.secondPage}</PaginationLink>
                        </PaginationItem>
                        <PaginationItem active={this.state.thirdPage === this.state.currentPage}>
                            <PaginationLink onClick={() => { this.onClick(this.state.thirdPage) }}>{this.state.thirdPage}</PaginationLink>
                        </PaginationItem>

                        <PaginationItem>
                            <PaginationLink next onClick={() => { this.onClick(this.state.currentPage + 1) }} />
                        </PaginationItem>
                    </Pagination>
                </div>
            );
        else
            if (this.state.pageCount === 2)
                return (
                    <div>
                        <Pagination>
                            <PaginationItem>
                                <PaginationLink previous onClick={() => { this.onClick(this.state.currentPage - 1) }} />
                            </PaginationItem>

                            <PaginationItem active={this.state.firstPage === this.state.currentPage}>
                                <PaginationLink onClick={() => { this.onClick(this.state.firstPage) }}>{this.state.firstPage}</PaginationLink>
                            </PaginationItem>
                            <PaginationItem active={this.state.secondPage === this.state.currentPage}>
                                <PaginationLink onClick={() => { this.onClick(this.state.secondPage) }}>{this.state.secondPage}</PaginationLink>
                            </PaginationItem>

                            <PaginationItem>
                                <PaginationLink next onClick={() => { this.onClick(this.state.currentPage + 1) }} />
                            </PaginationItem>
                        </Pagination>
                    </div>
                );
            else return (<></>);
    }

}

