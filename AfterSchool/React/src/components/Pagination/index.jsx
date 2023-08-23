import React, { useEffect, useState } from 'react';
import { makeStyles } from '@material-ui/core/styles';
import { Pagination, PaginationItem } from '@material-ui/lab';
import { useSearchParams } from 'react-router-dom';
import { Button } from '@material-ui/core';
import { startCase } from 'lodash';
import { buildQueryParams } from 'utils';

const useStyles = makeStyles(theme => ({
  root: {
    '& > *': {
      marginTop: theme.spacing(2)
    },
    '& .MuiPaginationItem-rounded': {
      borderRadius: 0
    }
  },
  itemSelected: {
    backgroundColor: `${theme.palette.primary.dark} !important`,
    color: 'white'
  },
  itemButton: {
    height: 32,
    borderRadius: 0
  }
}));

const DEFAULT_PAGE = 1;
export default function PaginationRounded({ totalPages = 50 }) {
  const classes = useStyles();
  const [searchParams, setSearchParams] = useSearchParams();
  const [currentPage, setCurrentPage] = useState(searchParams.get('pageNo'));

  const handlePageChange = (_event, value) => {
    setCurrentPage(value);
    setSearchParams(buildQueryParams('pageNo', value, searchParams));
  };

  useEffect(() => {
    if (!searchParams.get('pageNo')) setSearchParams(buildQueryParams('pageNo', DEFAULT_PAGE));
    setCurrentPage(Number(searchParams.get('pageNo')) || DEFAULT_PAGE);
  }, []);

  const renderItem = item => {
    if (['previous', 'next'].includes(item.type))
      return (
        <Button {...item} className={classes.itemButton}>
          {startCase(item.type)}
        </Button>
      );
    return (
      <PaginationItem
        classes={{
          selected: classes.itemSelected
        }}
        {...item}
        style={{ margin: 0 }}
      />
    );
  };

  return (
    <div className={classes.root}>
      <Pagination
        count={totalPages}
        page={currentPage}
        variant="outlined"
        onChange={handlePageChange}
        shape="rounded"
        renderItem={renderItem}
      />
    </div>
  );
}
